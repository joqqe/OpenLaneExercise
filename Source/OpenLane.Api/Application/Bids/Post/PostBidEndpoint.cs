﻿using FluentValidation;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Common;
using OpenLane.Api.Domain;
using OpenLane.Api.Common.Factories;

namespace OpenLane.Api.Application.Bids.Post
{
	public static class PostBidEndpoint
	{
		public const string Instance = "/Api/Bid";

		public static WebApplication UsePostBidEndpoint(this WebApplication app)
		{
			app.MapPost(Instance, async (
				IValidator<PostBidRequest> validator,
				IHandler<PostBidRequest, Result<Bid>> handler,
				PostBidRequest request) =>
			{
				var problemDetails = await validator.GetProblemDetailsAsync(request, Instance);
				if (problemDetails is not null)
					return Results.Problem(problemDetails);

				var response = await handler.InvokeAsync(request);
				if (response.IsFailure)
					return Results.Problem(response.Error, Instance, StatusCodes.Status400BadRequest, "A functional exception has occured.");

				return Results.Created(string.Format(GetBidEndpoint.Instance, response.Value!.ObjectId), request);
			})
			.WithName("PostBid")
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status400BadRequest)
			.ProducesValidationProblem()
			.WithOpenApi();

			return app;
		}
	}
}