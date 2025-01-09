using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Application.Bids.Post;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OpenLane.ApiTests.Endpoints;

public class PostEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly ApiWebApplicationFactory _application;

	public PostEndpointTests(ApiWebApplicationFactory application)
	{
		_client = application.CreateClient();
		_application = application;
	}

	[Fact]
	public async Task PostBids_ShouldReturn_201Created2Accepted()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var bidObjectId = Guid.NewGuid();
		var bidPrice = 120m;
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyObject = new PostBidRequest(bidObjectId, ApiWebApplicationFactory.OpenOffer.ObjectId, bidPrice, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");

		_client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

		// Act
		var postResponse = await _client.PostAsync(requestUri, bodyString);

		// Assert
		postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
		postResponse.Headers?.Location?.OriginalString.Should().Be(string.Format(GetBidEndpoint.InstanceFormat, bidObjectId));

		(await harness.Published.Any<Domain.Messages.BidReceivedMessage>()).Should().Be(true);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("123")]
	public async Task PostBids_Invalid_IdempotencyKey_ShouldFail(string idempotency)
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var bidObjectId = Guid.NewGuid();
		var bidPrice = 120m;
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyObject = new PostBidRequest(bidObjectId, ApiWebApplicationFactory.OpenOffer.ObjectId, bidPrice, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");

		_client.DefaultRequestHeaders.Add("Idempotency-Key", idempotency);

		// Act
		var postResponse = await _client.PostAsync(requestUri, bodyString);

		// Assert
		postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
	
	[Theory]
	[MemberData(nameof(GetBadRequestData))]
	public async Task PostBids_ShouldReturn_400BadRequest(PostBidRequest bodyObject)
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");
		_client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

		var response = await _client.PostAsync(requestUri, bodyString);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		(await harness.Published.Any<Domain.Messages.BidReceivedMessage>()).Should().Be(false);
	}
	public static IEnumerable<object[]> GetBadRequestData()
	{
		yield return new object[] { new PostBidRequest(Guid.Empty, Guid.NewGuid(), 120m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), Guid.Empty, 120m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), Guid.NewGuid(), 0, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), Guid.NewGuid(), 120m, Guid.Empty) };
	}
}
