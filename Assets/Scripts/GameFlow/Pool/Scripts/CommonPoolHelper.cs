using System.Collections.Generic;
using Drawmasters.Pool.Interfaces;
using UnityEngine;


namespace Drawmasters.Pool
{
    public class CommonPoolHelper<TKey, TComponent> : IPoolHelper<TKey, TComponent> where TComponent : MonoBehaviour
    {
        #region Fields

        private readonly Dictionary<TKey, ComponentPool> pools;

        private readonly Dictionary<TKey, AssetLink> assets;

        #endregion



        #region IPoolHelper

        public TComponent PopObject(TKey key)
        {
            TComponent popped = null;

            if (pools.TryGetValue(key, out ComponentPool pool))
            {
                popped = pool.Pop() as TComponent;
            }
            else if (assets.TryGetValue(key, out AssetLink link))
            {
                Object assetOriginal = link.GetAsset();

                if (!assetOriginal.IsNull())
                {
                    TComponent assetComponent = (assetOriginal as GameObject).GetComponent<TComponent>();

                    if (assetComponent != null)
                    {
                        ComponentPool createdPool = PoolManager.Instance.GetComponentPool(assetComponent);

                        pools.Add(key, createdPool);

                        popped = createdPool.Pop() as TComponent;

                    }
                    else
                    {
                        PoolManager.Log("Resource doesn't contain " + nameof(TComponent) + " component. Name = " + key.ToString());
                    }
                }
                else
                {
                    PoolManager.Log("Asset with GUID: " + link.assetGUID + " was not loaded.");
                }
            }
            else
            {
                PoolManager.Log("Cannot find asset. Name = " + key.ToString() + ".");
            }

            return popped;
        }


        public void PushObject(TComponent component)
        {
            ComponentPool pool = PoolManager.Instance.GetComponentPool(component, false);

            if (pool != null)
            {
                if (pool.CanHandlePush(component))
                {
                    pool.Push(component);
                }
                else if (pool.InPool(component))
                {
                    PoolManager.Log($"{component.GetType().Name} already in pool.");
                }
            }
            else if (!component.IsNull())
            {
                Object.Destroy(component.gameObject);
            }
        }

        #endregion



        #region Ctor

        public CommonPoolHelper(Dictionary<TKey, AssetLink> _assets)
        {
            pools = new Dictionary<TKey, ComponentPool>();

            assets = _assets;
        }

        #endregion
    }
}

