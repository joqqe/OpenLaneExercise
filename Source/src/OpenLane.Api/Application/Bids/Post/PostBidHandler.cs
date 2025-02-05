using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using OpenLane.Common;
using OpenLane.Common.Interfaces;
using OpenLane.Domain;
using OpenLane.Domain.Messages;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidCommand(Guid IdempotencyKey, Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId)
	: IdempotencyBase(IdempotencyKey);

public record PostBidResult(Result Result);

public class PostBidHandler : IHandler<PostBidCommand, Result>
{
	private readonly ILogger<PostBidHandler> _logger;
	private readonly IPublishEndpoint _publishEndpoint;

	public PostBidHandler(ILogger<PostBidHandler> logger, IPublishEndpoint publishEndpoint)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(publishEndpoint);

		_logger = logger;
		_publishEndpoint = publishEndpoint;
	}

	public async Task<Result> InvokeAsync(PostBidCommand request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		var message = new BidReceivedMessage(request.IdempotencyKey, request.BidObjectId, request.OfferObjectId, request.Price, request.UserObjectId);
		await _publishEndpoint.Publish(message, cancellationToken);

		_logger.LogInformation("Successfuly send bid accepted message.");

		return Result.Success();
	}
}
