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

		// To be removed!
		var message = await response.Content.ReadAsStringAsync();

		response.StatusCode.Should().Be(HttpStatusCode.Created);
	}
}
