namespace OpenLane.Common.Helpers;

public static class TypeHelper
{
	public static bool ImplementsInterface(this Type t, Type usedInterface)
	{
		var interfaceType = t.GetInterface(usedInterface.Name);
		if (interfaceType is null)
			return false;

		if (interfaceType.Namespace != usedInterface.Namespace)
			return false;

		return true;
	}
}
