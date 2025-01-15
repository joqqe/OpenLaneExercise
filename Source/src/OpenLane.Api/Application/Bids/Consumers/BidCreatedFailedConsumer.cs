using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenLane.Api.Hub;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;
using OpenLane.Domain.Services;
using System.Text.Json;

namespace OpenLane.Api.Application.Bids.Consumers;

public class BidCreatedFailedConsumer : IConsumer<BidCreatedFailedMessage>
{
	public const string IdempotencyTransaction = "BidCreatedFailedNotification";

	private readonly ILogger<BidCreatedFailedConsumer> _logger;
	private readonly IHubContext<NotificationHub> _hub;
	private readonly IIdempotencyService _idempotencyService;

	public BidCreatedFailedConsumer(ILogger<BidCreatedFailedConsumer> logger, IHubContext<NotificationHub> hub,
		IIdempotencyService idempotencyService)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(hub);
		ArgumentNullException.ThrowIfNull(idempotencyService);

		_logger = logger;
		_hub = hub;
		_idempotencyService = idempotencyService;
	}

	public async Task Consume(ConsumeContext<BidCreatedFailedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedFailedConsumer), JsonSerializer.Serialize(context.Message));

		if (await _idempotencyService.IsRequestProcessedAsync(context.Message.IdempotencyKey.ToString(), IdempotencyTransaction))
		{
			_logger.LogWarning("Duplicate message: {IdempontencyKey}.", context.Message.IdempotencyKey);
			return;
		}

		var notification = new BidCreatedFailedNotification(context.Message.BidObjectId, context.Message.ErrorMessage);
		await _hub.Clients
			.Group(context.Message.UserObjectId.ToString())
			.SendAsync("BidCreatedFailed", notification);

		await _idempotencyService.MarkRequestAsProcessedAsync(context.Message.IdempotencyKey.ToString(), IdempotencyTransaction);

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidCreatedFailedConsumer), JsonSerializer.Serialize(context.Message));

	}
}
