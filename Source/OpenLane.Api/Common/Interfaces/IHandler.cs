namespace OpenLane.Api.Common.Interfaces
{
	public interface IHandler<TRequest, TResponse>
	{
		Task<TRequest> InvokeAsync(TResponse request);
	}
}
