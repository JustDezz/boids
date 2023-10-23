using System;
using UnityEngine;

namespace Tools.RestrictTypeAttribute
{
	[AttributeUsage(AttributeTargets.Field)]
	public class RestrictAttribute : PropertyAttribute
	{
		public Type Type { get; }

		public RestrictAttribute(Type type) => Type = type;
	}
}