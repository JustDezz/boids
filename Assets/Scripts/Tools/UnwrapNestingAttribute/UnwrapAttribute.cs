using System;
using UnityEngine;

namespace Tools.UnwrapNestingAttribute
{
	[AttributeUsage(AttributeTargets.Field)]
	public class UnwrapAttribute : PropertyAttribute
	{
		public readonly bool FullUnwrap;

		public UnwrapAttribute(bool fullUnwrap = false) => FullUnwrap = fullUnwrap;
	}
}