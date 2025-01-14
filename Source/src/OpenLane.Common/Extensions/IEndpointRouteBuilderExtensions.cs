using OpenLane.Common.Helpers;
using OpenLane.Common.Interfaces;
using System.Reflection;

namespace OpenLane.Common.Extensions;

public static class IEndpointRouteBuilderExtensions
{
	public static void UseEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
	{
		foreach (var type in assembly.GetTypes().Where(t =>
			t.ImplementsInterface(typeof(IEndpoint))
			&& !t.IsAbstract
			&& !t.IsInterface))
		{
			var instance = (IEndpoint)Activator.CreateInstance(type)!;
			instance.UseEndpoint(app);
		}
	}
}