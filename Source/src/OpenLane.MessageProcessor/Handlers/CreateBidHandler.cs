using OpenLane.Domain;
using OpenLane.Infrastructure;
using OpenLane.Common;
using Microsoft.EntityFrameworkCore;
using OpenLane.Common.Interfaces;

namespace OpenLane.MessageProcessor.Handlers;

public record CreateBidRequest(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);

public class CreateBidHandler : IHandler<CreateBidRequest, Result<Bid>>
{
	private readonly ILogger<CreateBidHandler> _logger;
	private readonly AppDbContext _appContext;

	public CreateBidHandler(ILogger<CreateBidHandler> logger, AppDbContext appContext)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(appContext);

		_logger = logger;
		_appContext = appContext;
	}

	public async Task<Result<Bid>> InvokeAsync(CreateBidRequest request, CancellationToken cancellationToken = default)
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
			ObjectId = request.BidObjectId,
			Offer = offer,
			OfferId = offer.Id,
			Price = request.Price,
			UserObjectId = request.UserObjectId,
			ReceivedAt = DateTimeOffset.Now
		};

		await _appContext.Bids.AddAsync(newBid, cancellationToken);
		await _appContext.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Successfuly created bid entity: {Entity}", newBid);

		return Result<Bid>.Success(newBid);
	}
}
