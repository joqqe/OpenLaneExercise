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

		return GetProblemDetails(validationResult.Errors.Select(x => x.ErrorMessage).ToArray(), instance);
	}

	public static HttpValidationProblemDetails GetProblemDetails(string[] errorMessages, string instance)
	{
		return new HttpValidationProblemDetails(new Dictionary<string, string[]>
		{
			{ "ValidationErrors", errorMessages }
		})
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation failed",
			Detail = "One of more validation errors occurred.",
			Instance = instance
		};
	}
}
