using FluentAssertions;
using OpenLane.Api.Application.Bids.Get;
using System.Net;

namespace OpenLane.ApiTests.Endpoints;

public class BidEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly HttpClient _client;

	public BidEndpointTests(ApiWebApplicationFactory application)
	{
		_client = application.CreateClient();
	}

	[Fact]
	public async Task GetBids_ShouldReturn_200OK()
	{
		var bidObjectId = Guid.NewGuid();
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, ApiWebApplicationFactory.BidObjectId);
		var response = await _client.GetAsync(requestUri);

		var message = await response.Content.ReadAsStringAsync();

		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetBids_ShouldReturn_404NotFound()
	{
		var bidObjectId = Guid.NewGuid();
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, Guid.NewGuid());
		var response = await _client.GetAsync(requestUri);

		var message = await response.Content.ReadAsStringAsync();

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetBids_ShouldReturn_400BadRequest()
	{
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, Guid.Empty);
		var response = await _client.GetAsync(requestUri);

		var message = await response.Content.ReadAsStringAsync();

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
