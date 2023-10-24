using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.ComponentsComposition
{
	public class ComponentsRoot : MonoBehaviour, IComponentsRoot
	{
		private readonly Dictionary<Type, Object> _components = new();

		public T Resolve<T>() where T : Object
		{
			Type type = typeof(T);
			bool hasValue = _components.TryGetValue(type, out Object cached);
			if (hasValue && cached != null) return (T) cached;

			T component = gameObject.GetComponentInChildren<T>();
			if (hasValue) _components[type] = component;
			else _components.Add(type, component);

			return component;
		}

		public bool TryResolve<T>(out T component) where T : Object
		{
			component = Resolve<T>();
			return component != null;
		}
	}
}