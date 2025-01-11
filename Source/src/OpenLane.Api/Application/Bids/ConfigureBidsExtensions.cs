using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Application.Bids.Post;
using OpenLane.Common.Helpers;

namespace OpenLane.Api.Application.Bids;

public static class ConfigureBidsExtensions
{
	public static IServiceCollection AddBids(this IServiceCollection services)
	{
		services.AddHandlers(typeof(ConfigureBidsExtensions));

		return services;
	}

	public static WebApplication UseBids(this WebApplication app)
	{
		app.UseGetBidEndpoint();
		app.UsePostBidEndpoint();

		return app;
	}
}
