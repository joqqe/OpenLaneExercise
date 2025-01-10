using FluentAssertions;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Application.Dtos;
using System.Net;
using System.Text.Json;

namespace OpenLane.ApiTests.Endpoints;

[Collection("EnvironmenCollection")]
public class GetBidEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly ApiWebApplicationFactory _application;

	public GetBidEndpointTests(ApiWebApplicationFactory application)
	{
		_client = application.CreateClient();
		_application = application;
	}

	[Fact]
	public async Task GetBids_ShouldReturn_200OK()
	{
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, _application.Bid.ObjectId);
		var response = await _client.GetAsync(requestUri);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var bidString = await response.Content.ReadAsStringAsync();
		var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var bid = JsonSerializer.Deserialize<BidDto>(bidString, jsonSerializerOptions);
		bid.Should().NotBeNull();
		bid!.ObjectId.Should().Be(_application.Bid.ObjectId);
		bid!.Price.Should().Be(_application.Bid.Price);
		bid!.OfferId.Should().Be(_application.OpenOffer.ObjectId);
	}

	[Fact]
	public async Task GetBids_ShouldReturn_404NotFound()
	{
		var bidObjectId = Guid.NewGuid();
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, Guid.NewGuid());
		var response = await _client.GetAsync(requestUri);

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetBids_ShouldReturn_400BadRequest()
	{
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, Guid.Empty);
		var response = await _client.GetAsync(requestUri);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
