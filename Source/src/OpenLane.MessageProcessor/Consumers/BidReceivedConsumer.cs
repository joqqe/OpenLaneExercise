using MassTransit;
using OpenLane.Domain.Messages;
using OpenLane.MessageProcessor.Handlers;
using System.Text.Json;
using OpenLane.Domain.Services;

namespace OpenLane.MessageProcessor.Consumers;

public class BidReceivedConsumer : IConsumer<BidReceivedMessage>
{
	public const string IdempotencyTransaction = "BidCreated";

	private readonly ILogger<BidReceivedConsumer> _logger;
	private readonly CreateBidHandler _handler;
	private readonly IBus _bus;
	private readonly IIdempotencyService _idempotencyService;

	public BidReceivedConsumer(ILogger<BidReceivedConsumer> logger,
		CreateBidHandler handler, IBus bus,
		IIdempotencyService idempotencyService)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(handler);
		ArgumentNullException.ThrowIfNull(bus);
		ArgumentNullException.ThrowIfNull(idempotencyService);

		_logger = logger;
		_handler = handler;
		_bus = bus;
		_idempotencyService = idempotencyService;
	}

	public async Task Consume(ConsumeContext<BidReceivedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));

		if (await _idempotencyService.IsRequestProcessedAsync(context.Message.IdempotencyKey.ToString(), IdempotencyTransaction))
		{
			var createdFailedMessage = new BidCreatedFailedMessage(context.Message.IdempotencyKey, context.Message.BidObjectId, string.Format("Duplicate message: {0}.", context.Message.IdempotencyKey));
			await _bus.Publish(createdFailedMessage);

			_logger.LogWarning("Failed to consume {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
			return;
		}

		var request = new CreateBidCommand(context.Message.BidObjectId, context.Message.OfferObjectId, context.Message.Price, context.Message.UserObjectId);
		var result = await _handler.InvokeAsync(request);

		if (result.IsFailure)
		{
			var createdFailedMessage = new BidCreatedFailedMessage(context.Message.IdempotencyKey, context.Message.BidObjectId, result.Error ?? "Failed to create bid.");
			await _bus.Publish(createdFailedMessage);

			_logger.LogWarning("Failed to consume {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
			return;
		}

		var createdMessage = new BidCreatedMessage(context.Message.IdempotencyKey, result.Value!.ObjectId, result.Value.Offer.ObjectId, result.Value.Price, result.Value.UserObjectId);
		await _bus.Publish(createdMessage);

		await _idempotencyService.MarkRequestAsProcessedAsync(context.Message.IdempotencyKey.ToString(), IdempotencyTransaction);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
	}
}