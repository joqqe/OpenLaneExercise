using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenLane.Api.Hub;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;
using System.Text.Json;

namespace OpenLane.Api.Application.Bids.Consumers;

public class BidCreatedFailedConsumer : IConsumer<BidCreatedFailedMessage>
{
	private readonly ILogger<BidCreatedFailedConsumer> _logger;
	private readonly IHubContext<NotificationHub> _hub;

	public BidCreatedFailedConsumer(ILogger<BidCreatedFailedConsumer> logger, IHubContext<NotificationHub> hub)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(hub);

		_logger = logger;
		_hub = hub;
	}

	public async Task Consume(ConsumeContext<BidCreatedFailedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedFailedConsumer), JsonSerializer.Serialize(context.Message));

		var notification = new BidCreatedFailedNotification(context.Message.BidObjectId, context.Message.ErrorMessage);
		await _hub.Clients
			.Group(context.Message.UserObjectId.ToString())
			.SendAsync("BidCreatedFailed", notification);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidCreatedFailedConsumer), JsonSerializer.Serialize(context.Message));
	}
}
