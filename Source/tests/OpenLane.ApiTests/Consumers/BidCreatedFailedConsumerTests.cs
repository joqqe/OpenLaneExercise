using FluentAssertions;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Api.Application.Bids.Consumers;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;

namespace OpenLane.ApiTests.Consumers;

[Collection("EnvironmenCollection")]
public class BidCreatedFailedConsumerTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly ApiWebApplicationFactory _application;

	public BidCreatedFailedConsumerTests(ApiWebApplicationFactory application)
	{
		_application = application;
	}

	private async Task<HubConnection> CreateHubConnectionAsync()
	{
		var connection = new HubConnectionBuilder()
			.WithUrl("http://127.0.0.1/api/notification", options =>
			{
				options.HttpMessageHandlerFactory = _ => _application.Server.CreateHandler();
			})
			.Build();

		await connection.StartAsync();
		return connection;
	}

	[Fact]
	public async Task BidCreatedFailedConsumer_Should_SendNotification()
	{
		var connection = await CreateHubConnectionAsync();
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var cancellationTokenSource = new CancellationTokenSource();

		// Arrange
		var message = new BidCreatedFailedMessage(
			Guid.NewGuid(), Guid.NewGuid(), "Failed to create bid.");

		BidCreatedFailedNotification notification = default!;
		connection.On<BidCreatedFailedNotification>("BidCreatedFailed", (message) =>
		{
			notification = message;
			cancellationTokenSource.Cancel();
		});

		// Act
		await harness.Bus.Publish(message);

		// Assert
		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidCreatedFailedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);

		try { await Task.Delay(3000, cancellationTokenSource.Token); }
		catch { }

		notification.Should().NotBeNull();
		notification!.BidObjectId.Should().Be(message.BidObjectId);
		notification.ErrorMessage.Should().Be(message.ErrorMessage);
	}

	[Fact]
	public async Task BidCreatedFailedConsumer_DoubleIdempotencyKey_NotSendNotification()
	{
		var connection = await CreateHubConnectionAsync();
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var cancellationTokenSource = new CancellationTokenSource();

		// Arrange
		var message = new BidCreatedFailedMessage(
			Guid.NewGuid(), Guid.NewGuid(), "Failed to create bid.");

		BidCreatedFailedNotification notification = default!;
		connection.On<BidCreatedFailedNotification>("BidCreatedFailed", (message) =>
		{
			notification = message;
			cancellationTokenSource.Cancel();
		});

		// Act first publish
		await harness.Bus.Publish(message);

		// Assert first publish
		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidCreatedFailedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);

		try { await Task.Delay(3000, cancellationTokenSource.Token); }
		catch { }

		notification.Should().NotBeNull();
		notification!.BidObjectId.Should().Be(message.BidObjectId);
		notification.ErrorMessage.Should().Be(message.ErrorMessage);

		// Arrange second publish
		notification = default!;
		cancellationTokenSource = new CancellationTokenSource();

		// Act second publish
		await harness.Bus.Publish(message);

		// Assert second publish
		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);
		var consumerHarness2 = harness.GetConsumerHarness<BidCreatedFailedConsumer>();
		(await consumerHarness2.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);

		try { await Task.Delay(3000, cancellationTokenSource.Token); }
		catch { }

		notification.Should().BeNull();
	}
}
