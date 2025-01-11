using FluentValidation;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Common.Factories;
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using OpenLane.Domain.Services;

namespace OpenLane.Api.Application.Bids.Post;

public record PostBidRequest(Guid BidObjectId, Guid OfferObjectId, decimal Price, Guid UserObjectId);

public static class PostBidEndpoint
{
	public const string InstanceFormat = "/Api/Bid";
	public const string Instance = InstanceFormat;
	public const string IdempotencyTransaction = "BidReceived";

	public static WebApplication UsePostBidEndpoint(this WebApplication app)
	{
		app.MapPost(Instance, async (
			[FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
			[FromServices] ILogger<Program> logger,
			[FromServices] IValidator<PostBidRequest> validator,
			[FromServices] PostBidHandler handler,
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

			if (await idempotencyService.IsRequestProcessedAsync(idempotencyKey!.ToString(), IdempotencyTransaction))
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

			var postBidRequest = new PostBidCommand(idempotencyKey, request.BidObjectId, request.OfferObjectId, request.Price, request.UserObjectId);
			await handler.InvokeAsync(postBidRequest, cancellationToken);

			await idempotencyService.MarkRequestAsProcessedAsync(idempotencyKey!.ToString(), IdempotencyTransaction);

			logger.LogInformation("Successfuly send bid accepted.");

			return Results.Accepted(string.Format(GetBidEndpoint.InstanceFormat, request.BidObjectId), request);
		})
		.WithName("PostBid")
		.Produces(StatusCodes.Status202Accepted)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status409Conflict)
		.ProducesValidationProblem()
		.WithOpenApi();

		return app;
	}
}
