using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Domain;
using AppContext = OpenLane.Api.Infrastructure.AppContext;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidRequest(Guid OfferObjectId, decimal Price, Guid User);

public class PostBidHandler : IHandler<PostBidRequest, Result<Bid>>
{
	private readonly AppContext _appContext;

	public PostBidHandler(AppContext appContext)
	{
		_appContext = appContext;
	}

	public async Task<Result<Bid>> InvokeAsync(PostBidRequest request)
	{
		var offer = await _appContext.Offers.SingleOrDefaultAsync(x => x.ObjectId == request.OfferObjectId);
		if (offer is null)
			return Result<Bid>.Failure("No offer has been found.");

		var isBidHigherThenPrevious = await _appContext.Bids.AnyAsync(x => x.User == request.User && x.Price < request.Price);
		if (!isBidHigherThenPrevious)
			return Result<Bid>.Failure("There is already a higher bid.");

		var newBid = new Bid
		{
			ObjectId = Guid.NewGuid(),
			Offer = offer,
			OfferId = offer.Id,
			Price = request.Price,
			User = request.User,
			ReceivedAt = DateTimeOffset.Now
		};

		await _appContext.Bids.AddAsync(newBid);
		await _appContext.SaveChangesAsync();

		return Result<Bid>.Success(newBid);
	}
}
