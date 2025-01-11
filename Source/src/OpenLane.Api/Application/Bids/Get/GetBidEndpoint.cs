using FluentValidation;
using OpenLane.Api.Common.Factories;
using OpenLane.Api.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace OpenLane.Api.Application.Bids.Get;

public static class GetBidEndpoint
{
	public const string InstanceFormat = "/Api/Bid/{0}";
	public static readonly string Instance = string.Format(InstanceFormat, "{objectId}");

	public static WebApplication UseGetBidEndpoint(this WebApplication app)
	{
		app.MapGet(Instance, async (
			[FromServices] ILogger<Program> logger,
			[FromServices] IValidator<GetBidQuery> validator,
			[FromServices] GetBidHandler handler,
			CancellationToken cancellationToken,
			Guid objectId) =>
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(validator);
			ArgumentNullException.ThrowIfNull(handler);

			var request = new GetBidQuery(objectId);

			var problemDetails = await validator.GetProblemDetailsAsync(request, Instance, cancellationToken);
			if (problemDetails is not null)
			{
				logger.LogWarning("Invalid request: {ErrorMessage}", string.Join(", ", problemDetails.Errors["ValidationErrors"]));
				return Results.Problem(problemDetails);
			}

			var response = await handler.InvokeAsync(request, cancellationToken);
			if (response.Result.IsFailure)
				return Results.Problem(response.Result.Error, Instance, StatusCodes.Status400BadRequest, "A functional exception has occured.");

			if (response.Result.Value is null)
				return Results.NotFound();

			var dto = new BidDto(response.Result.Value.ObjectId, response.Result.Value.Price, response.Result.Value.Offer.ObjectId);
			logger.LogInformation("Successfuly send bid dto.");
			return Results.Ok(dto);
		})
		.WithName("GetBid")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.ProducesValidationProblem()
		.WithOpenApi();

		return app;
	}
}
