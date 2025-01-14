namespace OpenLane.Common.Interfaces;

public interface IEndpoint
{
	IEndpointRouteBuilder UseEndpoint(IEndpointRouteBuilder app);
}
