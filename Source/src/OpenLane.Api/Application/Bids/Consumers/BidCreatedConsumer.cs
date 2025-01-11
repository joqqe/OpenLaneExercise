using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenLane.Api.Hub;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;
using OpenLane.Domain.Services;
using OpenLane.Infrastructure.Services;
using System.Text.Json;

namespace OpenLane.Api.Application.Bids.Consumers;

public class BidCreatedConsumer : IConsumer<BidCreatedMessage>
{
	private readonly ILogger<BidCreatedConsumer> _logger;
	private readonly IHubContext<NotificationHub> _hub;
	private readonly IIdempotencyService _idempotencyService;

	public BidCreatedConsumer(ILogger<BidCreatedConsumer> logger, IHubContext<NotificationHub> hub,
		IIdempotencyService idempotencyService)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(hub);
		ArgumentNullException.ThrowIfNull(idempotencyService);

		_logger = logger;
		_hub = hub;
		_idempotencyService = idempotencyService;
	}

	public async Task Consume(ConsumeContext<BidCreatedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));

		if (await _idempotencyService.IsRequestProcessedAsync(context.Message.IdempotencyKey.ToString(), "BidCreatedNotification"))
		{
			_logger.LogWarning("Duplicate message: {IdempontencyKey}.", context.Message.IdempotencyKey);
			return;
		}

		var notification = new BidCreatedNotification(context.Message.BidObjectId, context.Message.OfferObjectId, context.Message.Price, context.Message.UserObjectId);
		await _hub.Clients.All.SendAsync("BidCreated", notification);

		await _idempotencyService.MarkRequestAsProcessedAsync(context.Message.IdempotencyKey.ToString(), "BidCreatedNotification");

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));
	}
}
