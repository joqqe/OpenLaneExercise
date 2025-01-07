using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Infrastructure;
using OpenLane.Domain;
using OpenLane.Domain.Messages;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidRequest(Guid OfferObjectId, decimal Price, Guid UserObjectId);

public class PostBidHandler : IHandler<PostBidRequest, Result<Bid>>
{
	private readonly ILogger<PostBidHandler> _logger;
	private readonly AppDbContext _appContext;
	private readonly IBus _bus;

	public PostBidHandler(ILogger<PostBidHandler> logger, AppDbContext appContext, IBus bus)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(appContext);
		ArgumentNullException.ThrowIfNull(bus);

		_logger = logger;
		_appContext = appContext;
		_bus = bus;
	}

	public async Task<Result<Bid>> InvokeAsync(PostBidRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var offer = await _appContext.Offers.SingleOrDefaultAsync(x => x.ObjectId == request.OfferObjectId, cancellationToken);
		if (offer is null)
		{
			var errorMessage = "No offer was been found.";
			_logger.LogWarning(errorMessage);
			return Result<Bid>.Failure(errorMessage);
		}

		var now = DateTimeOffset.Now;
		if (offer.OpensAt > now || offer.ClosesAt < now)
		{
			var errorMessage = "The offer is not open to receive bids.";
			_logger.LogWarning(errorMessage);
			return Result<Bid>.Failure(errorMessage);
		}

		if (offer.StartingPrice > request.Price)
		{
			var errorMessage = "The bid price should be higher than the starting price.";
			_logger.LogWarning(errorMessage);
			return Result<Bid>.Failure(errorMessage);
		}

		var isBidLowerOrEqualThenPrevious = await _appContext.Bids
			.AsNoTracking()
			.AnyAsync(x => x.UserObjectId == request.UserObjectId && x.Price >= request.Price, cancellationToken);
		if (isBidLowerOrEqualThenPrevious)
		{
			var errorMessage = "There is already a higher bid.";
			_logger.LogWarning(errorMessage);
			return Result<Bid>.Failure(errorMessage);
		}

		var newBid = new Bid
		{
			ObjectId = Guid.NewGuid(),
			Offer = offer,
			OfferId = offer.Id,
			Price = request.Price,
			UserObjectId = request.UserObjectId,
			ReceivedAt = DateTimeOffset.Now
		};

		await _appContext.Bids.AddAsync(newBid, cancellationToken);
		await _appContext.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Successfuly created bid entity: {Entity}", newBid);

		await _bus.Publish(new BidCreatedMessage(newBid.ObjectId, newBid.Price, newBid.Offer.ObjectId), cancellationToken);

		_logger.LogInformation("Successfuly send bid created message.");

		return Result<Bid>.Success(newBid);
	}
}
