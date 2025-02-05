using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using OpenLane.Infrastructure;
using MassTransit;
using OpenLane.MessageProcessor.Consumers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenLane.MessageProcessorTests.Environment;

namespace OpenLane.MessageProcessorTests;

[Collection(nameof(EnvironmentCollection))]
public class MessageProcessorWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly EnvironmentContainersFixture _environmentContainers;

	public MessageProcessorWebApplicationFactory(EnvironmentContainersFixture environmentContainers)
	{
		_environmentContainers = environmentContainers;
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

			services.AddMassTransitTestHarness(cfg =>
			{
				cfg.AddConsumer<BidReceivedConsumer>();
			});
		});

		builder.ConfigureTestServices(services =>
		{
			var sp = services.BuildServiceProvider();

			using var scope = sp.CreateScope();
			var scopedServices = scope.ServiceProvider;
			var appDbContext = scopedServices.GetRequiredService<AppDbContext>();

			appDbContext.Database.EnsureCreated();
		});
	}
}
