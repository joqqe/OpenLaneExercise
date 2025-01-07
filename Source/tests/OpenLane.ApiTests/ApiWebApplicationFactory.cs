using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenLane.Domain;
using OpenLane.Infrastructure;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace OpenLane.ApiTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public static readonly Offer OpenOffer = TestDataFactory.CreateOpenOffer();
	public static readonly Offer ClosedOffer = TestDataFactory.CreateClosedOffer();
	public static readonly Offer FutureOffer = TestDataFactory.CreateFutureOffer();
	public static readonly Bid Bid = TestDataFactory.CreateBid(OpenOffer);
	private readonly MsSqlContainer _msSqlContainer;
	private readonly RabbitMqContainer _rabbitMqContainer;

	public ApiWebApplicationFactory()
	{
		_msSqlContainer = new MsSqlBuilder().Build();
		_rabbitMqContainer = new RabbitMqBuilder().Build();

		Task.WhenAll([_msSqlContainer.StartAsync(), _rabbitMqContainer.StartAsync()])
			.GetAwaiter().GetResult();
	}

	public override async ValueTask DisposeAsync()
	{
		await Task.WhenAll([_msSqlContainer.StopAsync(), _rabbitMqContainer.StopAsync()]);
		await Task.WhenAll([_msSqlContainer.DisposeAsync().AsTask(), _rabbitMqContainer.DisposeAsync().AsTask()]);

		await base.DisposeAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration(config =>
		{
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					{ "ConnectionStrings:AppDB", _msSqlContainer.GetConnectionString() },
					{ "ConnectionStrings:MessageQueue", _rabbitMqContainer.GetConnectionString() }
				})
				.Build();

			config.AddConfiguration(configuration);
		});

		builder.ConfigureTestServices(services =>
		{
			var sp = services.BuildServiceProvider();

			using var scope = sp.CreateScope();
			var scopedServices = scope.ServiceProvider;
			var appDbContext = scopedServices.GetRequiredService<AppDbContext>();

			appDbContext.Database.EnsureCreated();

			SeedDatabase(appDbContext);
		});
	}

	private void SeedDatabase(AppDbContext appDbContext)
	{
		appDbContext.Offers.Add(OpenOffer);
		appDbContext.Offers.Add(ClosedOffer);
		appDbContext.Offers.Add(FutureOffer);
		appDbContext.Bids.Add(Bid);

		appDbContext.SaveChanges();
	}
}
