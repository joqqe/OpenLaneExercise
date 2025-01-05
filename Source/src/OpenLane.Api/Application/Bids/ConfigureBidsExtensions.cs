using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Application.Bids.Post;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Domain;

namespace OpenLane.Api.Application.Bids;

public static class ConfigureBidsExtensions
{
	public static IServiceCollection AddBids(this IServiceCollection services)
	{
		services.AddScoped<IHandler<GetBidRequest, Result<Bid?>>, GetBidHandler>();
		services.AddScoped<IHandler<PostBidRequest, Result<Bid>>, PostBidHandler>();

		return services;
	}

	public static WebApplication UseBids(this WebApplication app)
	{
		app.UseGetBidEndpoint();
		app.UsePostBidEndpoint();

		return app;
	}
}
