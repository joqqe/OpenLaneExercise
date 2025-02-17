using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenLane.Api.Hub;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;
using System.Text.Json;

namespace OpenLane.Api.Application.Bids.Consumers;

public class BidCreatedConsumer : IConsumer<BidCreatedMessage>
{
	private readonly ILogger<BidCreatedConsumer> _logger;
	private readonly IHubContext<NotificationHub> _hub;

	public BidCreatedConsumer(ILogger<BidCreatedConsumer> logger, IHubContext<NotificationHub> hub)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(hub);

		_logger = logger;
		_hub = hub;
	}

	public async Task Consume(ConsumeContext<BidCreatedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));

		var notification = new BidCreatedNotification(context.Message.BidObjectId, context.Message.OfferObjectId, context.Message.Price);
		await _hub.Clients
			.Group(context.Message.UserObjectId.ToString())
			.SendAsync("BidCreated", notification);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));
	}
}
