using UnityEngine;
using System.Collections.Generic;
using Drawmasters.Pool.Interfaces;


namespace Drawmasters.Pool
{
    public class PoolManager : SingletonMonoBehaviour<PoolManager>, IPoolManager
    {
        #region Constants

        internal static readonly Vector3 PoolPosition = new Vector3(0f, -20000f, 0f);

        #endregion



        #region Fields

        [SerializeField] private bool isDebuggingMode = default;

        internal readonly List<ComponentPool> componentPool = new List<ComponentPool>();

        private List<PoolManagerComponentTemplate> components;

        #endregion



        #region Properties

        public static bool IsDebugging => SingletonMonoBehaviour<PoolManager>.Instance.isDebuggingMode;

        public static new IPoolManager Instance => SingletonMonoBehaviour<PoolManager>.Instance;

        #endregion



        #region Unity lifecycle

        private void Start()
        {
            InitializeComponents();
        }

        private void Update()
        {
            foreach (var item in components)
            {
                if(item is IUpdatable updatable)
                {
                    updatable.CustomUpdate(Time.deltaTime);
                }
            }
        }

        #endregion



        #region IPoolManager

        public ComponentPool GetComponentPool(MonoBehaviour prefab, bool autoCreate = true, int preInstantiateCount = 1)
        {
            if (prefab.IsNull())
            {
                Log("Prefab is NULL.");                

                return null;
            }

		    ComponentPool pool = FindPool(prefab);

            bool needCreate = pool == null && autoCreate;

		    if (needCreate)
            {
                pool = CreateComponentPool(prefab, preInstantiateCount);
		    }

		    return pool;            
        }

        #endregion



        #region Component pool implementation

        private ComponentPool FindPool(MonoBehaviour prefab) 
        {
            ComponentPool pool = componentPool.Find(i => i.CanHandle(prefab));

            return pool;
	    }


        private ComponentPool CreateComponentPool(MonoBehaviour prefab, int count)
        {
            Transform anchor = CreateAnchor(prefab.name);

            PoolData poolData = new PoolData(anchor, count, true);

            ComponentPool pool = new ComponentPool(prefab, poolData);

            pool.Initialize();

            componentPool.Add(pool);

            return pool;
        }

        #endregion



        #region Private methods

        private void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<PoolManagerComponentTemplate>()
                {
                    //new PoolManagerComponentLifeTime(10),

                    #if UNITY_EDITOR
                    new PoolManagerComponentSorter(0.5f),
                    #endif

                };
            }

            foreach (var component in components)
            {
                component.Initialize(this);
            }
        }

        private Transform CreateAnchor(string name)
        {
            GameObject poolRootGameObject = new GameObject();
            Transform poolRoot = poolRootGameObject.transform;

            #if UNITY_EDITOR
                poolRoot.name = $"POOL_{name}";
            #endif

            poolRoot.position = PoolPosition;
            poolRoot.SetParent(transform);

            return poolRoot;
        }
        #endregion



        #region Debugging

        internal static void Log(string msg)
        {
            if (IsDebugging)
            {
                CustomDebug.Log(msg);
            }
        }

        #endregion
    }
}