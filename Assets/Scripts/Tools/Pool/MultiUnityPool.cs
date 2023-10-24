using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Tools.Pool
{
	public class MultiUnityPool<T> : IPool<T> where T : Component
	{
		private const string NullObjectErrorMessage = "Null object pool is not supported";
		private const string EmptyPrefabsArrayErrorMessage = "Empty prefabs array is not supported";

		internal readonly T[] Prefabs;
		internal readonly Queue<T> ObjectPool;
		protected readonly Transform PoolTransform;

		protected readonly Func<T, Transform, T> Instantiator;

		public MultiUnityPool(IEnumerable<T> prefabs, string poolName = "", Func<T, Transform, T> instantiator = null) 
			: this(prefabs, 0, poolName, instantiator) { }
		public MultiUnityPool(IEnumerable<T> prefabs, int initialSize, string poolName = "", Func<T, Transform, T> instantiator = null)
		{
			if (prefabs == null) throw new NotSupportedException(NullObjectErrorMessage);

			ObjectPool = new Queue<T>();
			if (string.IsNullOrWhiteSpace(poolName)) poolName = typeof(T).Name + " Pool";
			GameObject holder = new(poolName);
			PoolTransform = holder.transform;
			holder.SetActive(false);

			Instantiator = instantiator ?? Object.Instantiate;

			Prefabs = prefabs.ToArray();
			if (Prefabs.Length == 0) throw new NotSupportedException(EmptyPrefabsArrayErrorMessage);
			for (int i = 0; i < Prefabs.Length; i++)
			{
				T prefab = Prefabs[i];
				if (prefab == null) throw new NotSupportedException(NullObjectErrorMessage);

				T original = Instantiator(prefab, PoolTransform);
				original.name = prefab.name;
				Prefabs[i] = original;
			}
			
			Add(initialSize);
		}

		public T Get()
		{
			if (ObjectPool.Count == 0) Add(1);
			T toGet = ObjectPool.Dequeue();
			return toGet;
		}

		public void Return(T toReturn)
		{
			toReturn.transform.SetParent(PoolTransform, true);
			ObjectPool.Enqueue(toReturn);
		}

		private void Add(int i)
		{
			for (int j = 0; j < i; j++)
			{
				T prefab = Prefabs[Random.Range(0, Prefabs.Length)];
				T toAdd = Instantiator(prefab, PoolTransform);
				ObjectPool.Enqueue(toAdd);
			}
		}
	}
}