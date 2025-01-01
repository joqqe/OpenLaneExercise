using FluentValidation;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Common.Factories;
using OpenLane.Api.Application.Dtos;
using OpenLane.Api.Common;
using OpenLane.Api.Domain;

namespace OpenLane.Api.Application.Bids.Get;

public static class GetBidEndpoint
{
	public const string Instance = "/Api/Bid/{ObjectId}";

	public static WebApplication UseGetBidEndpoint(this WebApplication app)
	{
		app.MapGet(Instance, async (
			IValidator<GetBidRequest> validator,
			IHandler<GetBidRequest, Result<Bid?>> handler,
			Guid objectId) =>
		{
			var request = new GetBidRequest(objectId);

			var problemDetails = await validator.GetProblemDetailsAsync(request, Instance);
			if (problemDetails is not null)
				return Results.Problem(problemDetails);

			var response = await handler.InvokeAsync(request);
			if (response.IsFailure)
				return Results.Problem(response.Error, Instance, StatusCodes.Status400BadRequest, "A functional exception has occured.");

			if (response.Value is null)
				return Results.NotFound();

			var dto = new BidDto(response.Value.ObjectId, response.Value.Price, response.Value.User);
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
