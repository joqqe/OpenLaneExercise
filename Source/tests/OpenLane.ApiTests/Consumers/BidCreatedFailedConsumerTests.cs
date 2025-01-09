using FluentAssertions;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Api.Application.Bids.Consumers;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLane.ApiTests.Consumers;

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
	public async Task BidCreatedFailedConsumer_ShouldSave_Bid()
	{
		var connection = await CreateHubConnectionAsync();
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var message = new BidCreatedFailedMessage(
			Guid.NewGuid(), "Failed to create bid.");

		BidCreatedFailedNotification notification = default!;
		connection.On<BidCreatedFailedNotification>("BidCreatedFailed", (message) =>
		{
			notification = message;
		});

		// Act
		await harness.Bus.Publish(message);

		// Assert
		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidCreatedFailedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedFailedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedFailedMessage>()).Should().Be(true);

		notification.Should().NotBeNull();
		notification!.BidObjectId.Should().Be(message.BidObjectId);
		notification.ErrorMessage.Should().Be(message.ErrorMessage);
	}
}
