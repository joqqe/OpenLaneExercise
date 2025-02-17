using MassTransit;
using OpenLane.Domain.Messages;
using OpenLane.MessageProcessor.Handlers;
using System.Text.Json;

namespace OpenLane.MessageProcessor.Consumers;

public class BidReceivedConsumer : IConsumer<BidReceivedMessage>
{
	public const string IdempotencyTransaction = "BidCreated";

	private readonly ILogger<BidReceivedConsumer> _logger;
	private readonly CreateBidHandler _handler;
	private readonly IPublishEndpoint _publishEndpoint;

	public BidReceivedConsumer(ILogger<BidReceivedConsumer> logger,
		CreateBidHandler handler, IPublishEndpoint publishEndpoint)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(handler);
		ArgumentNullException.ThrowIfNull(publishEndpoint);

		_logger = logger;
		_handler = handler;
		_publishEndpoint = publishEndpoint;
	}

	public async Task Consume(ConsumeContext<BidReceivedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));

		var request = new CreateBidCommand(context.Message.BidObjectId, context.Message.OfferObjectId, context.Message.Price, context.Message.UserObjectId);
		var result = await _handler.InvokeAsync(request);

		if (result.IsFailure)
		{
			var createdFailedMessage = new BidCreatedFailedMessage(
				context.Message.BidObjectId,
				result.Error ?? "Failed to create bid.",
				context.Message.UserObjectId);
			await _publishEndpoint.Publish(createdFailedMessage);

			_logger.LogWarning("Failed to consume {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
			return;
		}

		var createdMessage = new BidCreatedMessage(result.Value!.ObjectId, result.Value.Offer.ObjectId, result.Value.Price, result.Value.UserObjectId);
		await _publishEndpoint.Publish(createdMessage);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
	}
}