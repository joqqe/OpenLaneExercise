using FluentValidation;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Common.Factories;
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using OpenLane.Domain.Messages;
using OpenLane.Common;
using OpenLane.Common.Interfaces;

namespace OpenLane.Api.Application.Bids.Post;

public static class PostBidEndpoint
{
	public const string InstanceFormat = "/Api/Bid";
	public const string Instance = InstanceFormat;

	public static WebApplication UsePostBidEndpoint(this WebApplication app)
	{
		app.MapPost(Instance, async (
			[FromServices] ILogger<Program> logger,
			[FromServices] IValidator<PostBidRequest> validator,
			[FromServices] IHandler<PostBidRequest, Result> handler,
			[FromServices] IBus bus,
			CancellationToken cancellationToken,
			PostBidRequest request) =>
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(validator);
			ArgumentNullException.ThrowIfNull(bus);

			var problemDetails = await validator.GetProblemDetailsAsync(request, Instance, cancellationToken);
			if (problemDetails is not null)
			{
				logger.LogWarning("Invalid request: {ErrorMessage}", string.Join(", ", problemDetails.Errors["ValidationErrors"]));
				return Results.Problem(problemDetails);
			}

			var postBidRequest = new PostBidRequest(request.BidObjectId, request.OfferObjectId, request.Price, request.UserObjectId);
			await handler.InvokeAsync(postBidRequest, cancellationToken);

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
