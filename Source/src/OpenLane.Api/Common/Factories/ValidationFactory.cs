using FluentValidation;

namespace OpenLane.Api.Common.Factories;

public static class ValidationFactory
{
	public static async Task<HttpValidationProblemDetails?> GetProblemDetailsAsync<T>(
		this IValidator<T> validator, T request, string instance, CancellationToken cancellationToken = default)
	{
		var validationResult = await validator.ValidateAsync(request, cancellationToken);

		if (validationResult.IsValid)
			return null;

		var problemDetails = new HttpValidationProblemDetails(new Dictionary<string, string[]>
		{
			{ "ValidationErrors", validationResult.Errors.Select(x => x.ErrorMessage).ToArray() }
		})
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation failed",
			Detail = "One of more validation errors occurred.",
			Instance = instance
		};

		return problemDetails;
	}
}
