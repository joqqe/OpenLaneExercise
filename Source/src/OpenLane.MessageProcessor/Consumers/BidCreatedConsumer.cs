using MassTransit;
using OpenLane.Domain.Messages;
using System.Text.Json;

namespace OpenLane.MessageProcessor.Consumers;

public class BidCreatedConsumer : IConsumer<BidCreatedMessage>
{
	private readonly ILogger<BidCreatedConsumer> _logger;

	public BidCreatedConsumer(ILogger<BidCreatedConsumer> logger)
	{
		ArgumentNullException.ThrowIfNull(logger);

		_logger = logger;
	}

	public Task Consume(ConsumeContext<BidCreatedMessage> context)
	{
		_logger.LogInformation("{Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));

		// Do some magic bid stuff here.

		_logger.LogInformation("Successfuly consumed {Consumer}: {Message}", nameof(BidCreatedConsumer), JsonSerializer.Serialize(context.Message));

		return Task.CompletedTask;
	}
}