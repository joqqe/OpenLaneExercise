using FluentValidation;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Common.Factories;
using Microsoft.AspNetCore.Mvc;
using OpenLane.Api.Application.Dtos;

namespace OpenLane.Api.Application.Bids.Get;

public static class GetBidEndpoint
{
	public static WebApplication UseGetBidEndpoint(this WebApplication app)
	{
		app.MapGet("/Api/Bid", async (
			IValidator<GetBidRequest> validator,
			IHandler<GetBidResponse?, GetBidRequest> handler,
			Guid objectId) =>
		{
			var request = new GetBidRequest(objectId);

			var problemDetails = await validator.GetProblemDetailsAsync(request, "/Api/Bid");
			if (problemDetails is not null)
				return Results.Problem(problemDetails);

			var response = await handler.InvokeAsync(request);
			if (response is null)
				return Results.NotFound();

			var dto = new BidDto(response.Bid.ObjectId, response.Bid.Price, response.Bid.User);
			return Results.Ok(dto);
		})
		.WithName("GetBid")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound)
		.ProducesValidationProblem()
		.WithOpenApi();

		return app;
	}
}
