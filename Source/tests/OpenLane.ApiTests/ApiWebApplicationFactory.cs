using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenLane.Domain;
using OpenLane.Infrastructure;
using MassTransit;
using OpenLane.Api.Application.Bids.Consumers;
using OpenLane.ApiTests.Helpers;

namespace OpenLane.ApiTests;

[Collection("EnvironmenCollection")]
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public readonly Offer OpenOffer;
	public readonly Offer ClosedOffer;
	public readonly Offer FutureOffer;
	public readonly Bid Bid;
	public readonly Guid UserObjectId;
	private readonly EnvironmentContainersFixture _environmentContainers;

	public ApiWebApplicationFactory(EnvironmentContainersFixture environmentContainers)
	{
		_environmentContainers = environmentContainers;

		OpenOffer = TestDataFactory.CreateOpenOffer();
		ClosedOffer = TestDataFactory.CreateClosedOffer();
		FutureOffer = TestDataFactory.CreateFutureOffer();
		Bid = TestDataFactory.CreateBid(OpenOffer);

		UserObjectId = Guid.NewGuid();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration(config =>
		{
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					{ "ConnectionStrings:AppDB", _environmentContainers.MsSqlContainer.GetConnectionString() },
					{ "ConnectionStrings:MessageQueue", _environmentContainers.RabbitMqContainer.GetConnectionString() },
					{ "ConnectionStrings:DistributedCache", _environmentContainers.RedisContainer.GetConnectionString() }
				})
				.Build();

			config.AddConfiguration(configuration);
		});

		builder.ConfigureServices(services =>
		{
			services.AddLogging(builder =>
				builder.ClearProviders()
			);

			services.AddSingleton<AccessTokenProvider>();

			services.AddMassTransitTestHarness(cfg =>
			{
				cfg.AddConsumer<BidCreatedConsumer>();
				cfg.AddConsumer<BidCreatedFailedConsumer>();
			});
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
