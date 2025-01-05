using Microsoft.EntityFrameworkCore;

namespace OpenLane.Api.Infrastructure;

public static class ConfigureInfraExtentions
{
	public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("AppDB"));
		});

		return services;
	}
}
