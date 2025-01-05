using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenLane.Domain;
using OpenLane.Api.Infrastructure;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace OpenLane.ApiTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public static readonly Guid BidObjectId = Guid.NewGuid();
	public static readonly Guid OfferObjectId = Guid.NewGuid();
	public static readonly Guid OfferObjectIdClosed = Guid.NewGuid();
	public static readonly Guid OfferObjectIdFuture = Guid.NewGuid();
	public static readonly Guid UserObjectId = Guid.NewGuid();
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

			appDbContext.Database.EnsureDeleted();
			appDbContext.Database.EnsureCreated();

			SeedDatabase(appDbContext);
		});
	}

	private void SeedDatabase(AppDbContext appDbContext)
	{
		// Open offer
		var productA = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductA"
		};
		var openOffer = new Offer
		{
			ObjectId = OfferObjectId,
			Product = productA,
			StartingPrice = 100m,
			OpensAt = DateTimeOffset.Now,
			ClosesAt = DateTimeOffset.Now.AddMonths(1)
		};
		var newBid = new Bid
		{
			ObjectId = BidObjectId,
			Offer = openOffer,
			Price = 110m,
			ReceivedAt = DateTimeOffset.Now,
			UserObjectId = UserObjectId
		};
		appDbContext.Bids.Add(newBid);

		// Closed offer
		var productB = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductB"
		};
		var closedOffer = new Offer
		{
			ObjectId = OfferObjectIdClosed,
			Product = productB,
			StartingPrice = 100m,
			OpensAt = DateTimeOffset.Now.AddMonths(-2),
			ClosesAt = DateTimeOffset.Now.AddMonths(-1)
		};
		appDbContext.Offers.Add(closedOffer);

		// Future offer
		var productC = new Product
		{
			ObjectId = Guid.NewGuid(),
			Name = "ProductC"
		};
		var futureOffer = new Offer
		{
			ObjectId = OfferObjectIdFuture,
			Product = productC,
			StartingPrice = 100m,
			OpensAt = DateTimeOffset.Now.AddMonths(1),
			ClosesAt = DateTimeOffset.Now.AddMonths(2)
		};
		appDbContext.Offers.Add(futureOffer);

		appDbContext.SaveChanges();
	}
}
