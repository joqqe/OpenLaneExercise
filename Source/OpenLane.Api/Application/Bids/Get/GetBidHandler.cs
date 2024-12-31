using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Domain;
using AppContext = OpenLane.Api.Infrastructure.AppContext;

namespace OpenLane.Api.Application.Bids.Get;

public record GetBidRequest(Guid ObjectId);

public class GetBidHandler : IHandler<GetBidRequest, Result<Bid?>>
{
	private readonly AppContext _appContext;

	public GetBidHandler(AppContext appContext)
	{
		_appContext = appContext;
	}

	public async Task<Result<Bid?>> InvokeAsync(GetBidRequest request)
	{
		var entity = await _appContext.Bids.SingleOrDefaultAsync(x => x.ObjectId == request.ObjectId);

		if (entity is null)
			return Result<Bid?>.Success(default);

		return Result<Bid?>.Success(entity);
	}
}
