using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using OpenLane.MessageProcessor.Consumers;
using OpenLane.MessageProcessorTests.Environment;
using OpenLane.MessageProcessorTests.Extensions;
using OpenLane.MessageProcessorTests.TestData;

namespace OpenLane.MessageProcessorTests.Consumers;

[Collection(nameof(EnvironmentCollection))]
public class BidReceivedConsumerTests : IClassFixture<MessageProcessorWebApplicationFactory>
{
	private readonly MessageProcessorWebApplicationFactory _application;

	public BidReceivedConsumerTests(MessageProcessorWebApplicationFactory application)
	{
		_application = application;
	}

	[Fact]
	public async Task BidReceivedConsumer_Should_SaveBid_SendBidCreatedMessage()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var objectMother = new ObjectMother();
		await _application.SeedDatabaseAsync(objectMother);
		var appDbContext = _application.GetAppDbContext();

		// Arrange
		var message = new BidReceivedMessage(
			Guid.NewGuid(), objectMother.OpenOffer.ObjectId, 120m, Guid.NewGuid());

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

	[Fact]
	public async Task BidReceivedConsumer_ToLowePrice_Should_NotSaveBid_SendBidCreatedFailedMessage()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var objectMother = new ObjectMother();
		await _application.SeedDatabaseAsync(objectMother);
		var appDbContext = _application.GetAppDbContext();

		// Arrange
		var message = new BidReceivedMessage(
			Guid.NewGuid(), objectMother.OpenOffer.ObjectId, objectMother.Bid.Price - 1, Guid.NewGuid());

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

	[Fact]
	public async Task BidReceivedConsumer_UnknownOffer_Should_NotSaveBid_SendBidCreatedFailedMessage()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var objectMother = new ObjectMother();
		await _application.SeedDatabaseAsync(objectMother);
		var appDbContext = _application.GetAppDbContext();

		// Arrange
		var message = new BidReceivedMessage(Guid.NewGuid(), Guid.NewGuid(), 120m, Guid.NewGuid());

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

	[Fact]
	public async Task BidReceivedConsumer_ClosedOffer_Should_NotSaveBid_SendBidCreatedFailedMessage()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var objectMother = new ObjectMother();
		await _application.SeedDatabaseAsync(objectMother); 
		var appDbContext = _application.GetAppDbContext();

		// Arrange
		var message = new BidReceivedMessage(Guid.NewGuid(), objectMother.ClosedOffer.ObjectId, 120m, Guid.NewGuid());

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

	[Fact]
	public async Task BidReceivedConsumer_FutureOffer_Should_NotSaveBid_SendBidCreatedFailedMessage()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var objectMother = new ObjectMother();
		await _application.SeedDatabaseAsync(objectMother);
		var appDbContext = _application.GetAppDbContext();

		// Arrange
		var message = new BidReceivedMessage(Guid.NewGuid(), objectMother.FutureOffer.ObjectId, 120m, Guid.NewGuid());

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
}
