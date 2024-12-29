using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Domain;
using AppContext = OpenLane.Api.Infrastructure.AppContext;

namespace OpenLane.Api.Application.Bids.Get;

public record GetBidRequest(Guid ObjectId);
public record GetBidResponse(Bid Bid);


public class GetBidHandler : IHandler<GetBidResponse?, GetBidRequest>
{
	private readonly AppContext _appContext;

	public GetBidHandler(AppContext appContext)
	{
		_appContext = appContext;
	}

	public async Task<GetBidResponse?> InvokeAsync(GetBidRequest request)
	{
		var entity = await _appContext.Bids.SingleOrDefaultAsync(x => x.ObjectId == request.ObjectId);

		if (entity is null)
			return null;

		return new GetBidResponse(entity);
	}
}
