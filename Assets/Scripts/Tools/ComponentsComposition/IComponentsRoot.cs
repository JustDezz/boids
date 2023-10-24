using UnityEngine;

namespace Tools.ComponentsComposition
{
	public interface IComponentsRoot
	{
		public T Resolve<T>() where T : Object;
		public bool TryResolve<T>(out T component) where T : Object;
	}
}