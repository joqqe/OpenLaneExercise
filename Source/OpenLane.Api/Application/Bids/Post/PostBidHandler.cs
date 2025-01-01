using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenLane.Api.Application.Bids.Consumers;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Domain;
using static Grpc.Core.Metadata;
using AppContext = OpenLane.Api.Infrastructure.AppContext;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidRequest(Guid OfferObjectId, decimal Price, Guid User);

public class PostBidHandler : IHandler<PostBidRequest, Result<Bid>>
{
	private readonly ILogger<PostBidHandler> _logger;
	private readonly AppContext _appContext;
	private readonly IBus _bus;

	public PostBidHandler(ILogger<PostBidHandler> logger, AppContext appContext, IBus bus)
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

		var isBidHigherThenPrevious = await _appContext.Bids.AnyAsync(x => x.User == request.User && x.Price < request.Price);
		if (!isBidHigherThenPrevious)
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
			User = request.User,
			ReceivedAt = DateTimeOffset.Now
		};

		await _appContext.Bids.AddAsync(newBid);
		await _appContext.SaveChangesAsync();

		_logger.LogInformation("Successfuly created bid entity: {Entity}", newBid);

		await _bus.Send(new BidCreatedMessage());

		_logger.LogInformation("Successfuly send bid created message.");

		return Result<Bid>.Success(newBid);
	}
}
