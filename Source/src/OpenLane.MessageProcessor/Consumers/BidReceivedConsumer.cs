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

	public BidReceivedConsumer(ILogger<BidReceivedConsumer> logger, IHandler<CreateBidRequest, Result<Bid>> handler)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(handler);

		_logger = logger;
		_handler = handler;
	}

	public async Task Consume(ConsumeContext<BidReceivedMessage> context)
	{
		// Todo: add validation

		_logger.LogInformation("{Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));

		var request = new CreateBidRequest(context.Message.BidObjectId, context.Message.OfferObjectId, context.Message.Price, context.Message.UserObjectId);
		await _handler.InvokeAsync(request);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidReceivedConsumer), JsonSerializer.Serialize(context.Message));
	}
}