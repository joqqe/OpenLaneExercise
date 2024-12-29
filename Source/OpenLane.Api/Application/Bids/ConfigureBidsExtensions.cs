using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Common.Interfaces;

namespace OpenLane.Api.Application.Bids;

public static class ConfigureBidsExtensions
{
	public static IServiceCollection AddBids(this IServiceCollection services)
	{
		services.AddScoped<IHandler<GetBidResponse?, GetBidRequest>>();

		return services;
	}

	public static WebApplication UseBids(this WebApplication app)
	{
		app.UseGetBidEndpoint();

		return app;
	}
}
