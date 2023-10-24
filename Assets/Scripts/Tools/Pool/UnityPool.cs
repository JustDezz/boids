using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.Pool
{
    public class UnityPool<T> : IPool<T> where T : Component
    {
        internal readonly T Prefab;
        internal readonly Queue<T> ObjectPool;
        protected readonly Transform PoolTransform;

        protected readonly Func<T, Transform, T> Instantiator;

        public UnityPool(T prefab, string poolName = "", Func<T, Transform, T> instantiator = null)
        {
            if (prefab == null) throw new NotSupportedException("Null object pool is not supported");
            
            ObjectPool = new Queue<T>();
            if (string.IsNullOrWhiteSpace(poolName)) poolName = typeof(T).Name + " Pool";
            GameObject holder = new(poolName);
            PoolTransform = holder.transform;
            holder.SetActive(false);

            Instantiator = instantiator ?? Object.Instantiate;

            Prefab = Instantiator(prefab, PoolTransform);
            Prefab.name = prefab.name;
        }

        public T Get()
        {
            if (ObjectPool.Count == 0) Add(1);
            T toGet = ObjectPool.Dequeue();
            return toGet;
        }

        public virtual void Return(T toReturn)
        {
            toReturn.transform.SetParent(PoolTransform, true);
            ObjectPool.Enqueue(toReturn);
        }

        protected virtual void Add(int i)
        {
            for (int j = 0; j < i; j++)
            {
                T toAdd = Instantiator(Prefab, PoolTransform);
                ObjectPool.Enqueue(toAdd);
            }
        }
    }
}