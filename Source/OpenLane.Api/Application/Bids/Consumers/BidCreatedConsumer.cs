using MassTransit;
using System.Text.Json;

namespace OpenLane.Api.Application.Bids.Consumers;

public record BidCreatedMessage();

public class BidCreatedConsumer : IConsumer<BidCreatedMessage>
{
	private readonly ILogger<BidCreatedConsumer> _logger;

	public BidCreatedConsumer(ILogger<BidCreatedConsumer> logger)
	{
		ArgumentNullException.ThrowIfNull(_logger);
		
		_logger = logger;
	}

	public Task Consume(ConsumeContext<BidCreatedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));

		// Do some magic bids stuff here.

		return Task.CompletedTask;
	}
}
