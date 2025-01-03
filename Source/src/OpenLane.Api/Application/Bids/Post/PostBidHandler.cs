using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Application.Dtos;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Domain;
using OpenLane.Api.Domain.Messages;
using OpenLane.Api.Infrastructure;

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

	public async Task<Result<Bid>> InvokeAsync(PostBidRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		var offer = await _appContext.Offers.SingleOrDefaultAsync(x => x.ObjectId == request.OfferObjectId);
		if (offer is null)
		{
			var errorMessage = "No offer has been found.";
			_logger.LogWarning(errorMessage);
			return Result<Bid>.Failure(errorMessage);
		}

		var now = DateTimeOffset.Now;
		if (offer.OpensAt > now && offer.ClosesAt < now)
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

		var isBidLowerOrEqualThenPrevious = await _appContext.Bids.AnyAsync(x => x.UserObjectId == request.UserObjectId && x.Price >= request.Price);
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

		await _appContext.Bids.AddAsync(newBid);
		await _appContext.SaveChangesAsync();

		_logger.LogInformation("Successfuly created bid entity: {Entity}", newBid);

		var newBidDto = new BidDto(newBid.ObjectId, newBid.Price, newBid.Offer.ObjectId);
		await _bus.Send(new BidCreatedMessage(newBidDto));

		_logger.LogInformation("Successfuly send bid created message.");

		return Result<Bid>.Success(newBid);
	}
}
