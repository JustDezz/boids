using System;

namespace Tools.Extensions
{
	public static class TypeExtensions
	{
		public static bool Extends(this Type type, Type other)
		{
			if (type == other) return true;
			
			Type baseType = type.BaseType;
			while (baseType != null)
			{
				if (baseType == other) return true;
				baseType = baseType.BaseType;
			}

			return false;
		}
	}
}