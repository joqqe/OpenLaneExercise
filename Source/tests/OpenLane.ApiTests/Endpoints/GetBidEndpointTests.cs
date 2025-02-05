using FluentAssertions;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Application.Dtos;
using OpenLane.ApiTests.Environment;
using OpenLane.ApiTests.Extensions;
using System.Net;
using System.Text.Json;

namespace OpenLane.ApiTests.Endpoints;

[Collection(nameof(EnvironmentCollection))]
public class GetBidEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
	private readonly HttpClient _client;
	private readonly ApiWebApplicationFactory _application;

	public GetBidEndpointTests(ApiWebApplicationFactory application)
	{
		ArgumentNullException.ThrowIfNull(application);

		_client = application.CreateClient();
		_application = application;
	}

	[Fact]
	public async Task GetBids_ShouldReturn_200OK()
	{
		var objectMother = new ObjectMother();
		var accessToken = _application.GetAccessToken(objectMother.UserObjectId);
		await _application.SeedDatabaseAsync(objectMother);

		// Arrange
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, objectMother.Bid.ObjectId);
		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
		
		// Act
		var response = await _client.GetAsync(requestUri);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var bidString = await response.Content.ReadAsStringAsync();
		var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var bid = JsonSerializer.Deserialize<BidDto>(bidString, jsonSerializerOptions);
		bid.Should().NotBeNull();
		bid!.ObjectId.Should().Be(objectMother.Bid.ObjectId);
		bid!.Price.Should().Be(objectMother.Bid.Price);
		bid!.OfferId.Should().Be(objectMother.OpenOffer.ObjectId);
	}

	[Fact]
	public async Task GetBids_ShouldReturn_404NotFound()
	{
		var objectMother = new ObjectMother();
		var accessToken = _application.GetAccessToken(objectMother.UserObjectId);
		await _application.SeedDatabaseAsync(objectMother);

		// Arrange
		var bidObjectId = Guid.NewGuid();
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, Guid.NewGuid());
		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

		// Act
		var response = await _client.GetAsync(requestUri);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetBids_ShouldReturn_400BadRequest()
	{
		var objectMother = new ObjectMother();
		var accessToken = _application.GetAccessToken(objectMother.UserObjectId);
		await _application.SeedDatabaseAsync(objectMother);

		// Arrange
		var requestUri = string.Format(GetBidEndpoint.InstanceFormat, Guid.Empty);
		_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
		
		// Act
		var response = await _client.GetAsync(requestUri);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}
