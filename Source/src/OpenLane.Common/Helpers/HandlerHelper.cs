using Microsoft.Extensions.DependencyInjection;

namespace OpenLane.Common.Helpers;

public static class HandlerHelper
{
	public static void AddHandlers(this IServiceCollection services, Type typeWithinNamespace)
	{
		foreach (var type in typeWithinNamespace.Assembly.GetTypes().Where(x =>
					x.IsInNamespace(typeWithinNamespace.Namespace!)
					&& x.Name.EndsWith("Handler")
					&& !x.IsAbstract
					&& !x.IsInterface))
		{
			services.AddTransient(type);
		}
	}

	public static bool IsInNamespace(this Type type, string nameSpace)
	{
		var subNameSpace = nameSpace + ".";
		return type.Namespace != null 
			&& (type.Namespace.Equals(nameSpace) 
			|| type.Namespace.StartsWith(subNameSpace));
	}
}
