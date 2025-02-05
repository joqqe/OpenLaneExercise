using OpenLane.Api.Common.Attributes;
using OpenLane.Api.Common.Factories;
using OpenLane.Domain.Services;
using System.Text.Json;

namespace OpenLane.Api.Common.Middleware;

public class IdempotencyMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<IdempotencyMiddleware> _logger;
	private readonly IIdempotencyService _idempotencyService;

	public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger, IIdempotencyService idempotencyService)
	{
		_next = next;
		_logger = logger;
		_idempotencyService = idempotencyService;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var idempotencyAttribute = context.GetEndpoint()?.Metadata.GetMetadata<IdempotencyAttribute>();
		var method = context.Request.Method;

		if (idempotencyAttribute is null
			&& method == HttpMethods.Post
			&& !context.Request.Path.StartsWithSegments("/api/notification")
			&& !context.Request.Path.StartsWithSegments("/api/health"))
		{
			var errorMessage = "This enpoint is missing an idempotency check.";
			_logger.LogWarning(errorMessage);
			await SetProblemDetails(context, errorMessage);
			return;
		}

		if (idempotencyAttribute is not null)
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

			if (await _idempotencyService.IsRequestProcessedAsync(idempotencyKey!.ToString(), idempotencyAttribute.TransactionKey))
			{
				var errorMessage = string.Format("Duplicate request: {0}.", idempotencyKey!);
				_logger.LogWarning(errorMessage);
				await SetProblemDetails(context, errorMessage, StatusCodes.Status409Conflict);
				return;
			}

			await _next(context);

			if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
			{
				await _idempotencyService.MarkRequestAsProcessedAsync(
					idempotencyKey!.ToString(), idempotencyAttribute.TransactionKey, TimeSpan.FromMinutes(idempotencyAttribute.ExpirationInMinutes));
			}

			return;
		}

		await _next(context);
	}

	private async Task SetProblemDetails(HttpContext context, string errorMessage, int statusCode = StatusCodes.Status400BadRequest)
	{
		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/problem+json";

		var problemDetails = ValidationFactory.GetProblemDetails([errorMessage], context.Request.Path);
		await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
	}
}
