using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OpenLane.Api.Hub;

[Authorize]
public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
{
	public override async Task OnConnectedAsync()
	{
		var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId != null)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, userId);
		}
		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (userId != null)
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
		}
		await base.OnDisconnectedAsync(exception);
	}
}
