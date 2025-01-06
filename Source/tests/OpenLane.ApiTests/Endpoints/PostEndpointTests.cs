using FluentAssertions;
using OpenLane.Api.Application.Bids.Post;
using OpenLane.Api.Application.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OpenLane.ApiTests.Endpoints;

public class PostEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly HttpClient _client;

	public PostEndpointTests(ApiWebApplicationFactory application)
	{
		_client = application.CreateClient();
	}

	[Fact]
	public async Task PostBids_ShouldReturn_201Created()
	{
		// Arrange
		var bidPrice = 120m;
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);
		var bodyObject = new PostBidRequest(ApiWebApplicationFactory.OpenOffer.ObjectId, bidPrice, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");
		
		// Act
		var postResponse = await _client.PostAsync(requestUri, bodyString);

		// Assert
		postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

		postResponse.Headers?.Location?.OriginalString.Should().NotBeNull();
		var getResponse = await _client.GetAsync(postResponse.Headers!.Location!.OriginalString);
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		var bidString = await getResponse.Content.ReadAsStringAsync();
		var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var bid = JsonSerializer.Deserialize<BidDto>(bidString, jsonSerializerOptions);
		bid.Should().NotBeNull();
		bid!.Price.Should().Be(bidPrice);
		bid!.OfferId.Should().Be(ApiWebApplicationFactory.OpenOffer.ObjectId);
	}

	[Theory]
	[MemberData(nameof(GetBadRequestData))]
	public async Task PostBids_ShouldReturn_400BadRequest(PostBidRequest bodyObject)
	{
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);

		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(requestUri, bodyString);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
	public static IEnumerable<object[]> GetBadRequestData()
	{
		yield return new object[] { new PostBidRequest(Guid.Empty, 120m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), 0, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), 120m, Guid.Empty) };
		yield return new object[] { new PostBidRequest(Guid.NewGuid(), 120m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.OpenOffer.ObjectId, 10m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.OpenOffer.ObjectId, 110m, ApiWebApplicationFactory.Bid.UserObjectId) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.ClosedOffer.ObjectId, 120m, ApiWebApplicationFactory.Bid.UserObjectId) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.FutureOffer.ObjectId, 120m, ApiWebApplicationFactory.Bid.UserObjectId) };
	}
}
