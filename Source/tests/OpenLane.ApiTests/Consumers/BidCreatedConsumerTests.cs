﻿using FluentAssertions;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Api.Application.Bids.Consumers;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;

namespace OpenLane.ApiTests.Consumers;

[Collection("EnvironmenCollection")]
public class BidCreatedConsumerTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly ApiWebApplicationFactory _application;

	public BidCreatedConsumerTests(ApiWebApplicationFactory application)
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
	public async Task BidCreatedConsumer_Should_SendNotification()
	{
		var connection = await CreateHubConnectionAsync();
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var cancellationTokenSource = new CancellationTokenSource();

		// Arrange
		var message = new BidCreatedMessage(
			Guid.NewGuid(),  Guid.NewGuid(), _application.OpenOffer.ObjectId, 120m, Guid.NewGuid());

		BidCreatedNotification notification = default!; 
		connection.On<BidCreatedNotification>("BidCreated", (message) =>
		{
			notification = message;
			cancellationTokenSource.Cancel();
		});

		// Act
		await harness.Bus.Publish(message);

		// Assert
		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidCreatedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);

		try { await Task.Delay(3000, cancellationTokenSource.Token); }
		catch { }

		notification.Should().NotBeNull();
		notification!.BidObjectId.Should().Be(message.BidObjectId);
		notification.OfferObjectId.Should().Be(message.OfferObjectId);
		notification.Price.Should().Be(message.Price);
		notification.UserObjectId.Should().Be(message.UserObjectId);
	}

	[Fact]
	public async Task BidCreatedConsumer_DoubleIdempotencyKey_NotSendNotification()
	{
		var connection = await CreateHubConnectionAsync();
		var harness = _application.Services.GetRequiredService<ITestHarness>();
		var cancellationTokenSource = new CancellationTokenSource();

		// Arrange
		var message = new BidCreatedMessage(
			Guid.NewGuid(), Guid.NewGuid(), _application.OpenOffer.ObjectId, 120m, Guid.NewGuid());

		BidCreatedNotification notification = default!;
		connection.On<BidCreatedNotification>("BidCreated", (message) =>
		{
			notification = message;
			cancellationTokenSource.Cancel();
		});

		// Act first publish
		await harness.Bus.Publish(message);

		// Assert first publish
		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidCreatedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);

		try { await Task.Delay(3000, cancellationTokenSource.Token); }
		catch { }

		notification.Should().NotBeNull();
		notification!.BidObjectId.Should().Be(message.BidObjectId);
		notification.OfferObjectId.Should().Be(message.OfferObjectId);
		notification.Price.Should().Be(message.Price);
		notification.UserObjectId.Should().Be(message.UserObjectId);

		// Arrange second publish
		notification = default!;
		cancellationTokenSource = new CancellationTokenSource();

		// Act second publish
		await harness.Bus.Publish(message);

		// Assert second publish
		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);
		var consumerHarness2 = harness.GetConsumerHarness<BidCreatedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);

		try { await Task.Delay(3000, cancellationTokenSource.Token); }
		catch { }

		notification.Should().BeNull();
	}
}