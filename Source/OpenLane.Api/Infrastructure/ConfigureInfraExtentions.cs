using Microsoft.EntityFrameworkCore;

namespace OpenLane.Api.Infrastructure;

public static class ConfigureInfraExtentions
{
	public static IServiceCollection AddInfra(this IServiceCollection services, ConfigurationManager configuration)
	{
		services.AddDbContext<AppContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("SQLServer"));
		});

		return services;
	}
}
