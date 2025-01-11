using OpenLane.Api.Common.Factories;
using System.Text.Json;

namespace OpenLane.Api.Common.Middleware;

public class IdempotencyCheckMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<IdempotencyCheckMiddleware> _logger;

	public IdempotencyCheckMiddleware(RequestDelegate next, ILogger<IdempotencyCheckMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Method == HttpMethods.Post 
			&& !context.Request.Path.StartsWithSegments("/api/notification")
			&& !context.Request.Path.StartsWithSegments("/api/health"))
		{
			if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
			{
				var errorMessage = "Idempotency-Key header is missing.";
				_logger.LogWarning(errorMessage);
				await SetProblemDetails(context, errorMessage);
				return;
			}
			if (string.IsNullOrWhiteSpace(idempotencyKey)
				|| !Guid.TryParse(idempotencyKey, out var idempotencyKeyGuid))
			{
				var errorMessage = "Invalid Idempotency-Key header.";
				_logger.LogWarning(errorMessage);
				await SetProblemDetails(context, errorMessage);
				return;
			}
		}

		await _next(context);
	}

	private async Task SetProblemDetails(HttpContext context, string errorMessage)
	{
		context.Response.StatusCode = StatusCodes.Status400BadRequest;
		context.Response.ContentType = "application/problem+json";

		var problemDetails = ValidationFactory.GetProblemDetails([errorMessage], context.Request.Path);
		await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
	}
}
