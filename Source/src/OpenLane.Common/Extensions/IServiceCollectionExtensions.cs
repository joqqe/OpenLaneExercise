using OpenLane.Common.Helpers;
using OpenLane.Common.Interfaces;
using System.Reflection;

namespace OpenLane.Common.Extensions;

public static class IServiceCollectionExtensions
{
	public static void AddHandlers(this IServiceCollection services, Assembly assembly)
	{
		foreach (var type in assembly.GetTypes().Where(t =>
			t.ImplementsInterface(typeof(IHandler<object, object>))
			&& !t.IsAbstract
			&& !t.IsInterface))
		{
			services.AddTransient(type);
		}
	}
}
