using MassTransit;
using OpenLane.Common.Interfaces;
using OpenLane.Common;
using OpenLane.Domain;
using OpenLane.Domain.Messages;
using OpenLane.MessageProcessor.Handlers;
using System.Text.Json;

namespace OpenLane.MessageProcessor.Consumers;

public class BidReceivedConsumer : IConsumer<BidReceivedMessage>
{
	private readonly ILogger<BidReceivedConsumer> _logger;
	private readonly IHandler<CreateBidRequest, Result<Bid>> _handler;
	private readonly IBus _bus;

	public BidReceivedConsumer(ILogger<BidReceivedConsumer> logger, IHandler<CreateBidRequest, Result<Bid>> handler, IBus bus)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(handler);

		_logger = logger;
		_handler = handler;
		_bus = bus;
	}

	public async Task Consume(ConsumeContext<BidReceivedMessage> context)
	{
		// Todo: add validation
		// Todo: add global exception handler

		_logger.LogInformation("{Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));

		var request = new CreateBidRequest(context.Message.BidObjectId, context.Message.OfferObjectId, context.Message.Price, context.Message.UserObjectId);
		var result = await _handler.InvokeAsync(request);

		if (result.IsFailure)
		{
			var createdFailedMessage = new BidCreatedFailedMessage(context.Message.BidObjectId, result.Error ?? "Failed to create bid.");
			await _bus.Publish(createdFailedMessage);

			_logger.LogWarning("Failed to consume {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
			return;
		}

		var createdMessage = new BidCreatedMessage(result.Value!.ObjectId, result.Value.Offer.ObjectId, result.Value.Price, result.Value.UserObjectId);
		await _bus.Publish(createdMessage);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
	}
}