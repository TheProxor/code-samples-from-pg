using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Drawmasters.Pool
{
    public abstract class Pool<T> where T : Object
    {
        #region Fields

        protected readonly T originalPrefab;

        protected readonly PoolData poolData;

        protected Stack<T> inPool;

        protected HashSet<int> outOfPool;

        internal event Action OnSomething;

        #endregion



        #region Properties

        internal int Count => inPool.Count;
        internal PoolData Data => poolData;

        #endregion



        #region Abstract methods

        abstract protected T CreateObject();

        abstract protected void HandlePushedObject(T pushed);

        abstract protected void HandlePoppedObject(T popped);

        #endregion



        #region Ctor

        internal Pool(T _originalPrefab, PoolData _poolData)
        {
            originalPrefab = _originalPrefab;
            poolData = _poolData;

            inPool = new Stack<T>();
            outOfPool = new HashSet<int>();
        }

        #endregion



        #region Public methods

        public virtual void Initialize()
        {
            for (int i = 0; i < poolData.PreInstantiatedElementsCount; i++)
            {
                PushObject(CreateObject(), true);
            }
        }

        public bool CanHandlePop(T poolable) => poolable != null && originalPrefab.GetInstanceID() == poolable.GetInstanceID();

        public bool CanHandlePush(T poolable) => poolable != null && outOfPool.Contains(poolable.GetInstanceID());

        public bool InPool(T poolable) => inPool.Contains(poolable);

        #endregion



        #region Protected methods

        protected void PushObject(T poolable, bool isFromPool = false)
        {
            if (PoolManager.IsDebugging)
            {
                if (outOfPool.Contains(poolable.GetInstanceID()) && isFromPool)
                {
                    PoolManager.Log("Recently created object is already marked such a one which is out of pool.");
                }

                if (!outOfPool.Contains(poolable.GetInstanceID()) && !isFromPool)
                {
                    PoolManager.Log("Try push unknwon object. It wasn't in pool and it is not created from pool.");
                }
            }

            if (outOfPool.Contains(poolable.GetInstanceID()))
            {
                outOfPool.Remove(poolable.GetInstanceID());
            }

            if (!inPool.Contains(poolable))
            {
                inPool.Push(poolable);
            }
            else
            {
                PoolManager.Log("Try push a object, which is in pool now.");
            }

            HandlePushedObject(poolable);
            OnSomething?.Invoke();
        }

        protected T PopObject()
        {
            T poolable = null;

            if (inPool.Count > 0)
            {
                poolable = inPool.Pop();
            }
            else if (poolData.IsAutoExtended)
            {
                poolable = CreateObject();
            }
            else
            {
                PoolManager.Log("Cannot pop object from pool.");
            }

            if (poolable != null)
            {
                if (!outOfPool.Contains(poolable.GetInstanceID()))
                {
                    outOfPool.Add(poolable.GetInstanceID());
                }

                HandlePoppedObject(poolable);
                OnSomething?.Invoke();
            }

            return poolable;
        }

        #endregion
    }
}