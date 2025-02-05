using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Domain.Services;
using OpenLane.Infrastructure.Factories;
using OpenLane.Infrastructure.Services;

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

		services.AddSingleton(sp => ConnectionMultiplexerFactory.CreateInstance(configuration));

		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = configuration.GetConnectionString("DistributedCache")!;
			options.ConnectionMultiplexerFactory = () => Task.FromResult(ConnectionMultiplexerFactory.CreateInstance(configuration));
		});

		return services;
	}
}
