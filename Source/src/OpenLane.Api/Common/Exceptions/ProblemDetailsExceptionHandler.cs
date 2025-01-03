using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OpenLane.Api.Common.Exceptions;

public class ProblemDetailsExceptionHandler : IExceptionHandler
{
	private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

	public ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger)
	{
		ArgumentNullException.ThrowIfNull(logger);
		_logger = logger;
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		int status = exception switch
		{
			ArgumentException => StatusCodes.Status400BadRequest,
			_ => StatusCodes.Status500InternalServerError
		};
		httpContext.Response.StatusCode = status;

		var problemDetails = new ProblemDetails
		{
			Status = status,
			Title = "An error occurred",
			Type = exception.GetType().Name,
			Detail = exception.Message
		};

		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

		_logger.LogError("Global exception occured: {ProblemDetails}", problemDetails);

		return true;
	}
}
