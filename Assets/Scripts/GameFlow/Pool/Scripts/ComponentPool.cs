using Drawmasters.Pool.Interfaces;
using UnityEngine;


namespace Drawmasters.Pool
{
    public class ComponentPool : Pool<MonoBehaviour>, IPool<MonoBehaviour>
    {
        #region Abstract implementation

        protected override MonoBehaviour CreateObject()
        {
            MonoBehaviour created = UnityEngine.Object.Instantiate(originalPrefab);

            #if UNITY_EDITOR
                created.gameObject.name = originalPrefab.gameObject.name;
            #endif

            return created;
        }


        protected override void HandlePushedObject(MonoBehaviour pushed)
        {
            pushed.transform.SetParent(poolData.RootTransform);
            pushed.transform.localPosition = Vector3.zero;

            if (pushed is IPoolCallback callbackImplementation)
            {
                callbackImplementation.OnPush();
            }

            pushed.gameObject.SetActive(false);
        }


        protected override void HandlePoppedObject(MonoBehaviour popped)
        {
            popped.transform.SetParent(null);

            if (popped is IPoolCallback callbackImplementation)
            {
                callbackImplementation.OnPop();
            }

            popped.gameObject.SetActive(true);
        }

        #endregion



        #region Public methods

        internal void RemoveFirst()
        {
            if (Count > 0)
            {
                var obj = inPool.Pop();

                if (!obj.IsNull())
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
        }

        #endregion



        #region Ctor

        public ComponentPool(MonoBehaviour originalPrefab, PoolData data)
            : base(originalPrefab, data){}

        #endregion



        #region IPool

        public bool CanHandle(MonoBehaviour prefab)
        {
            if (prefab.IsNull())
            {
                PoolManager.Log("Prefab is NULL.");

                return false;
            }
            else
            {
                return CanHandlePush(prefab) || CanHandlePop(prefab) || InPool(prefab);
            }
        }


        public MonoBehaviour Pop() => PopObject();


        public void Push(MonoBehaviour prefab)
        {
            if (!prefab.IsNull())
            {
                PushObject(prefab);
            }
            else
            {
                PoolManager.Log("Cannot push component. It is NULL.");
            }
        }

        #endregion
    }
}
