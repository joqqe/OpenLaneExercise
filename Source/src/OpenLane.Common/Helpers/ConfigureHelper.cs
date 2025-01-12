using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace OpenLane.Common.Helpers;

public static class ConfigureHelper
{
	public static void AddHandlers(this IServiceCollection services, Type typeWithinNamespace)
	{
		foreach (var type in typeWithinNamespace.Assembly.GetTypes().Where(x =>
			x.Name.EndsWith("Handler")
			&& !x.IsAbstract
			&& !x.IsInterface))
		{
			services.AddTransient(type);
		}
	}

	public static void UseEndpoints(this IApplicationBuilder app, Type typeWithinNamespace)
	{
		foreach (var type in typeWithinNamespace.Assembly.GetTypes().Where(x =>
			x.Name.EndsWith("Endpoint")
			&& x.IsAbstract 
			&& x.IsSealed))
		{
			var methodName = $"Use{type.Name}"; 
			var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

			methodInfo?.Invoke(type, [app]);
		}
	}
}
