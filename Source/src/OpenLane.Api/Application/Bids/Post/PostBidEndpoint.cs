﻿using FluentValidation;
using OpenLane.Api.Application.Bids.Get;
using OpenLane.Api.Common.Interfaces;
using OpenLane.Api.Common;
using OpenLane.Api.Common.Factories;
using Microsoft.AspNetCore.Mvc;
using OpenLane.Domain;

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
			[FromServices] IHandler<PostBidRequest, Result<Bid>> handler,
			CancellationToken cancellationToken,
			PostBidRequest request) =>
		{
			ArgumentNullException.ThrowIfNull(validator);
			ArgumentNullException.ThrowIfNull(handler);

			var problemDetails = await validator.GetProblemDetailsAsync(request, Instance, cancellationToken);
			if (problemDetails is not null)
			{
				logger.LogWarning("Invalid request: {ErrorMessage}", string.Join(", ", problemDetails.Errors["ValidationErrors"]));
				return Results.Problem(problemDetails);
			}

			var response = await handler.InvokeAsync(request, cancellationToken);
			if (response.IsFailure)
				return Results.Problem(response.Error, Instance, StatusCodes.Status400BadRequest, "A functional exception has occured.");

			logger.LogInformation("Successfuly send created response.");

			return Results.Created(string.Format(GetBidEndpoint.InstanceFormat, response.Value!.ObjectId), request);
		})
		.WithName("PostBid")
		.Produces(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.ProducesValidationProblem()
		.WithOpenApi();

		return app;
	}
}
