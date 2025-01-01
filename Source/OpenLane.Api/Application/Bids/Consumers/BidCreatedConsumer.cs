using MassTransit;
using Microsoft.AspNetCore.SignalR;
using OpenLane.Api.Application.Bids.Hubs;
using System.Text.Json;

namespace OpenLane.Api.Application.Bids.Consumers;

public record BidCreatedMessage();

public class BidCreatedConsumer : IConsumer<BidCreatedMessage>
{
	private readonly ILogger<BidCreatedConsumer> _logger;
	private readonly IHubContext<BidHub> _hubContext;

	public BidCreatedConsumer(ILogger<BidCreatedConsumer> logger, IHubContext<BidHub> hubContext)
	{
		ArgumentNullException.ThrowIfNull(_logger);
		
		_logger = logger;
		_hubContext = hubContext;
	}

	public Task Consume(ConsumeContext<BidCreatedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));

		_hubContext.Clients.All.SendAsync("Bid", context.Message);

		// Do some magic bid stuff here.

		return Task.CompletedTask;
	}
}
