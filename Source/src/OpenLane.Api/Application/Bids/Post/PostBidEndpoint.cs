using FluentValidation;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Common.Factories;
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using OpenLane.Common;
using OpenLane.Common.Interfaces;
using OpenLane.Domain.Services;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidRequest(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);

public static class PostBidEndpoint
{
	public const string InstanceFormat = "/Api/Bid";
	public const string Instance = InstanceFormat;

	public static WebApplication UsePostBidEndpoint(this WebApplication app)
	{
		app.MapPost(Instance, async (
			[FromServices] ILogger<Program> logger,
			[FromServices] IValidator<PostBidRequest> validator,
			[FromServices] IHandler<PostBidHandleRequest, Result> handler,
			[FromServices] IBus bus,
			[FromServices] IIdempotencyService idempotencyService,
			CancellationToken cancellationToken,
			HttpRequest httpRequest,
			PostBidRequest request) =>
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(validator);
			ArgumentNullException.ThrowIfNull(handler);
			ArgumentNullException.ThrowIfNull(bus);
			ArgumentNullException.ThrowIfNull(idempotencyService);

			if (!httpRequest.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
			{
				var errorMessage = "Idempotency-Key header is missing.";
				logger.LogWarning(errorMessage);
				return Results.BadRequest(errorMessage);
			}
			if (string.IsNullOrWhiteSpace(idempotencyKey)
				|| !Guid.TryParse(idempotencyKey, out var idempotencyKeyGuid))
			{
				var errorMessage = "Invalid Idempotency-Key header.";
				logger.LogWarning(errorMessage);
				return Results.BadRequest(errorMessage);
			}
			if (await idempotencyService.IsRequestProcessedAsync(idempotencyKey!, "ReceivedBid"))
			{
				var errorMessage = string.Format("Duplicate request: {0}.", idempotencyKey!);
				logger.LogWarning(errorMessage);
				return Results.Conflict(errorMessage);
			}

			var problemDetails = await validator.GetProblemDetailsAsync(request, Instance, cancellationToken);
			if (problemDetails is not null)
			{
				logger.LogWarning("Invalid request: {ErrorMessage}", string.Join(", ", problemDetails.Errors["ValidationErrors"]));
				return Results.Problem(problemDetails);
			}

			var postBidRequest = new PostBidHandleRequest(idempotencyKeyGuid, request.BidObjectId, request.OfferObjectId, request.Price, request.UserObjectId);
			await handler.InvokeAsync(postBidRequest, cancellationToken);

			await idempotencyService.MarkRequestAsProcessedAsync(idempotencyKey!, "ReceivedBid");

			logger.LogInformation("Successfuly send bid accepted.");

			return Results.Accepted(string.Format(GetBidEndpoint.InstanceFormat, request.BidObjectId), request);
		})
		.WithName("PostBid")
		.Produces(StatusCodes.Status202Accepted)
		.Produces(StatusCodes.Status400BadRequest)
		.ProducesValidationProblem()
		.WithOpenApi();

		return app;
	}
}
