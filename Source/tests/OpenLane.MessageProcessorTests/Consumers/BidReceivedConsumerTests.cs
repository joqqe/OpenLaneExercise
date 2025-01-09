using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Domain.Messages;
using OpenLane.Infrastructure;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using OpenLane.MessageProcessor.Consumers;

namespace OpenLane.MessageProcessorTests.Consumers;

public class BidReceivedConsumerTests : IClassFixture<MessageProcessorWebApplicationFactory>
{
	private readonly MessageProcessorWebApplicationFactory _application;

	public BidReceivedConsumerTests(MessageProcessorWebApplicationFactory application)
	{
		_application = application;
	}

	[Fact]
	public async Task BidReceivedConsumer_ShouldSave_Bid()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var serviceScopeFactory = _application.Services.GetRequiredService<IServiceScopeFactory>();
		using var scope = serviceScopeFactory.CreateScope();
		var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		// Arrange
		var message = new BidReceivedMessage(
			Guid.NewGuid(), MessageProcessorWebApplicationFactory.OpenOffer.ObjectId, 120m, Guid.NewGuid());

		// Act
		await harness.Bus.Publish(message);

		// Assert
		(await harness.Published.Any<BidReceivedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidReceivedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidReceivedConsumer>();
		(await consumerHarness.Consumed.Any<BidReceivedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);

		var bid = await appDbContext.Bids
			.Include(x => x.Offer)
			.SingleOrDefaultAsync(x => x.ObjectId == message.BidObjectId);
		bid.Should().NotBeNull();
		bid?.ObjectId.Should().Be(message.BidObjectId);
		bid?.Offer.ObjectId.Should().Be(message.OfferObjectId);
		bid?.Price.Should().Be(message.Price);
		bid?.UserObjectId.Should().Be(message.UserObjectId);
	}

	[Theory]
	[MemberData(nameof(GetBadMessageData))]
	public async Task BidReceivedConsumer_ShouldNotSave_Bid(BidReceivedMessage message)
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var serviceScopeFactory = _application.Services.GetRequiredService<IServiceScopeFactory>();
		using var scope = serviceScopeFactory.CreateScope();
		var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		// Act
		await harness.Bus.Publish(message);

		// Assert
		(await harness.Published.Any<BidReceivedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidReceivedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidReceivedConsumer>();
		(await consumerHarness.Consumed.Any<BidReceivedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);

		var bid = await appDbContext.Bids
			.Include(x => x.Offer)
			.SingleOrDefaultAsync(x => x.ObjectId == message.BidObjectId);
		bid.Should().BeNull();
	}

	public static IEnumerable<object[]> GetBadMessageData()
	{
		yield return new object[] { new BidReceivedMessage(
			Guid.NewGuid(), Guid.NewGuid(), 120m, Guid.NewGuid()) };
		yield return new object[] { new BidReceivedMessage(
			Guid.NewGuid(), MessageProcessorWebApplicationFactory.ClosedOffer.ObjectId, 120m, Guid.NewGuid()) };
		yield return new object[] { new BidReceivedMessage(
			Guid.NewGuid(), MessageProcessorWebApplicationFactory.FutureOffer.ObjectId, 120m, Guid.NewGuid()) };
		yield return new object[] { new BidReceivedMessage(
			Guid.NewGuid(), MessageProcessorWebApplicationFactory.OpenOffer.ObjectId, 50m, Guid.NewGuid()) };
	}
}
