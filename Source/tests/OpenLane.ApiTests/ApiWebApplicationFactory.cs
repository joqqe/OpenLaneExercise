using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenLane.Api.Domain;
using OpenLane.Api.Infrastructure;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace OpenLane.ApiTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public static readonly Guid BidObjectId = Guid.NewGuid();
	public static readonly Guid OfferObjectId = Guid.NewGuid();
	private readonly MsSqlContainer _msSqlContainer;
	private readonly RabbitMqContainer _rabbitMqContainer;

	public ApiWebApplicationFactory()
	{
		_msSqlContainer = new MsSqlBuilder().Build();
		_msSqlContainer.StartAsync().GetAwaiter().GetResult();

		_rabbitMqContainer = new RabbitMqBuilder().WithUsername("guest").WithPassword("guest").Build();
		_rabbitMqContainer.StartAsync().GetAwaiter().GetResult();
	}

	public override async ValueTask DisposeAsync()
	{
		await _msSqlContainer.StopAsync();
		await _msSqlContainer.DisposeAsync();

		await _rabbitMqContainer.StopAsync();
		await _rabbitMqContainer.DisposeAsync();

		await base.DisposeAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration(config =>
		{
			_rabbitMqContainer.GetConnectionString();

			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					{ "ConnectionStrings:AppDB", _msSqlContainer.GetConnectionString() },
					{ "MessageQueue:Host", _rabbitMqContainer.Hostname },
					{ "MessageQueue:Username", "guest" },
					{ "MessageQueue:Password", "guest" }
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
		var newProduct = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductA"
		};
		var newOffer = new Offer
		{
			ObjectId = OfferObjectId,
			Product = newProduct,
			StartingPrice = 100m,
			OpensAt = DateTimeOffset.Now,
			ClosesAt = DateTimeOffset.Now.AddMonths(1)
		};
		var newBid = new Bid
		{
			ObjectId = BidObjectId,
			Offer = newOffer,
			Price = 110m,
			ReceivedAt = DateTimeOffset.Now,
			UserObjectId = Guid.NewGuid()
		};

		appDbContext.Bids.Add(newBid);
		appDbContext.SaveChanges();
	}
}
