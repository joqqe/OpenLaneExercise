using MassTransit;
using OpenLane.Common;
using OpenLane.Common.Interfaces;
using OpenLane.Domain;
using OpenLane.Domain.Messages;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidRequest(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);

public class PostBidHandler : IHandler<PostBidRequest, Result>
{
	private readonly ILogger<PostBidHandler> _logger;
	private readonly IBus _bus;

	public PostBidHandler(ILogger<PostBidHandler> logger, IBus bus)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(bus);

		_logger = logger;
		_bus = bus;
	}

	public async Task<Result> InvokeAsync(PostBidRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var message = new BidReceivedMessage(request.BidObjectId, request.OfferObjectId, request.Price, request.UserObjectId);
		await _bus.Publish(message, cancellationToken);

		_logger.LogInformation("Successfuly send bid accepted message.");

		return Result<Bid>.Success();
	}
}
