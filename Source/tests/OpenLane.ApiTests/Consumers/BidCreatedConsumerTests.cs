using FluentAssertions;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Api.Application.Bids.Consumers;
using OpenLane.Domain.Messages;
using OpenLane.Domain.Notifications;
using System.Text.Json;

namespace OpenLane.ApiTests.Consumers;

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
	public async Task BidCreatedConsumer_ShouldSave_Bid()
	{
		var connection = await CreateHubConnectionAsync();
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var message = new BidCreatedMessage(
			Guid.NewGuid(), ApiWebApplicationFactory.OpenOffer.ObjectId, 120m, Guid.NewGuid());

		BidCreatedNotification notification = default!; 
		connection.On<BidCreatedNotification>("BidCreated", (message) =>
		{
			notification = message;
		});

		// Act
		await harness.Bus.Publish(message);

		// Assert
		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);
		(await harness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);
		var consumerHarness = harness.GetConsumerHarness<BidCreatedConsumer>();
		(await consumerHarness.Consumed.Any<BidCreatedMessage>()).Should().Be(true);

		(await harness.Published.Any<BidCreatedMessage>()).Should().Be(true);

		notification.Should().NotBeNull();
		notification!.BidObjectId.Should().Be(message.BidObjectId);
		notification.OfferObjectId.Should().Be(message.OfferObjectId);
		notification.Price.Should().Be(message.Price);
		notification.UserObjectId.Should().Be(message.UserObjectId);
	}
}