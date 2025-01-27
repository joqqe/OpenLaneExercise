using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Domain.Services;
using OpenLane.Infrastructure.Services;
using StackExchange.Redis;

namespace OpenLane.Infrastructure;

public static class ConfigureInfraExtentions
{
	public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("AppDB"));
		});

		services.AddDistributedCache(configuration);

		services.AddTransient<IIdempotencyService, IdempotencyService>();
		services.AddTransient<ILockService, LockService>();

		return services;
	}

	private static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
	{
		// This complex setup is done, so AddRedisInstrumentation will produce trace logging.

		var connectionStringDistributedCache = configuration.GetConnectionString("DistributedCache")!;
		var connectionMultiplexer = (IConnectionMultiplexer)ConnectionMultiplexer.Connect(connectionStringDistributedCache);
		
		services.AddSingleton(connectionMultiplexer);

		services.AddStackExchangeRedisCache(options =>
		{
			options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
		});

		return services;
	}
}
