using Microsoft.AspNetCore.SignalR.Client;

namespace OpenLane.ApiTests.Helpers;

public static class SignalRHelper
{
	public static async Task<HubConnection> CreateHubConnectionAsync(ApiWebApplicationFactory app, string url, string accessToken)
	{
		var connection = new HubConnectionBuilder()
			.WithUrl(url, options =>
			{
				options.HttpMessageHandlerFactory = _ => app.Server.CreateHandler();
				options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
			})
			.Build();

		await connection.StartAsync();
		return connection;
	}
}
