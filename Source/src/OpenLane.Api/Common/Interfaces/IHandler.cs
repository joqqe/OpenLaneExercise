namespace OpenLane.Api.Common.Interfaces;

public interface IHandler<TRequest, TResponse>
{
	Task<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken = default);
}
