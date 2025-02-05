using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Application.Bids.Post;
using OpenLane.ApiTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OpenLane.ApiTests.Endpoints;

[Collection(nameof(EnvironmentCollection))]
public class PostEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly ApiWebApplicationFactory _application;
	private readonly string _accessToken;

	public PostEndpointTests(ApiWebApplicationFactory application)
	{
		_client = application.CreateClient();
		_application = application;
		_accessToken = application.Services.GetRequiredService<AccessTokenProvider>()
			.GetToken(_application.UserObjectId.ToString());
	}

	[Fact]
	public async Task PostBids_ShouldReturn_201Created2Accepted()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var bidObjectId = Guid.NewGuid();
		var bidPrice = 120m;
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyObject = new PostBidRequest(bidObjectId, _application.OpenOffer.ObjectId, bidPrice, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");
		
		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
		_client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

		// Act
		var postResponse = await _client.PostAsync(requestUri, bodyString);

		// Assert
		postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
		postResponse.Headers?.Location?.OriginalString.Should().Be(string.Format(GetBidEndpoint.InstanceFormat, bidObjectId));

		(await harness.Published.Any<Domain.Messages.BidReceivedMessage>()).Should().Be(true);
	}
	
	[Theory]
	[MemberData(nameof(GetBadRequestData))]
	public async Task PostBids_ShouldReturn_400BadRequest(PostBidRequest bodyObject)
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");
		
		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
		_client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());

		var response = await _client.PostAsync(requestUri, bodyString);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
	public static IEnumerable<object[]> GetBadRequestData()
	{
		yield return new object[] { new PostBidRequest(Guid.Empty, Guid.NewGuid(), 120m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), Guid.Empty, 120m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), Guid.NewGuid(), 0, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), Guid.NewGuid(), 120m, Guid.Empty) };
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("123")]
	public async Task PostBids_Invalid_IdempotencyKey_ShouldFail(string? idempotency)
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var bidObjectId = Guid.NewGuid();
		var bidPrice = 120m;
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyObject = new PostBidRequest(bidObjectId, _application.OpenOffer.ObjectId, bidPrice, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");

		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
		_client.DefaultRequestHeaders.Add("Idempotency-Key", idempotency);

		// Act
		var postResponse = await _client.PostAsync(requestUri, bodyString);

		// Assert
		postResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task PostBids_DoubleIdempotencyKey_ShouldReturn_409Conflict()
	{
		var harness = _application.Services.GetRequiredService<ITestHarness>();

		// Arrange
		var idempotencyKey = Guid.NewGuid().ToString();
		var bidObjectId = Guid.NewGuid();
		var bidPrice = 120m;
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyObject = new PostBidRequest(bidObjectId, _application.OpenOffer.ObjectId, bidPrice, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");

		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
		_client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

		// Act first call
		var postResponse = await _client.PostAsync(requestUri, bodyString);

		// Assert first call
		postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
		(await harness.Published.Any<Domain.Messages.BidReceivedMessage>()).Should().Be(true);

		// Act second call
		var postResponse2 = await _client.PostAsync(requestUri, bodyString);

		// Assert second call
		postResponse2.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}
}
