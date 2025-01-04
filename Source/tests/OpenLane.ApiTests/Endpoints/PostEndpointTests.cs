using FluentAssertions;
using OpenLane.Api.Application.Bids.Post;
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
		var requestUri = string.Format(PostBidEndpoint.InstanceFormat);

		var bodyObject = new PostBidRequest(ApiWebApplicationFactory.OfferObjectId, 120m, Guid.NewGuid());
		var bodyString = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");
		var response = await _client.PostAsync(requestUri, bodyString);

		response.StatusCode.Should().Be(HttpStatusCode.Created);
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
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.OfferObjectId, 10m, Guid.NewGuid()) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.OfferObjectId, 110m, ApiWebApplicationFactory.UserObjectId) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.OfferObjectIdClosed, 120m, ApiWebApplicationFactory.UserObjectId) };
		yield return new object[] { new PostBidRequest(ApiWebApplicationFactory.OfferObjectIdFuture, 120m, ApiWebApplicationFactory.UserObjectId) };
	}
}
