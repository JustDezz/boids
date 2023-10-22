using UnityEngine;

namespace Tools.UnwrapNestingAttribute
{
	public class UnwrapAttribute : PropertyAttribute
	{
		public readonly bool FullUnwrap;

		public UnwrapAttribute(bool fullUnwrap = false) => FullUnwrap = fullUnwrap;
	}
}