using Drawmasters.Pool.Interfaces;
using UnityEngine;


namespace Drawmasters.Pool
{
    public class PoolData
    {
        #region Properties

        internal Transform RootTransform { get; }

        internal int PreInstantiatedElementsCount { get; }

        internal bool IsAutoExtended { get; }

        #endregion



        #region Ctor

        public PoolData(Transform rootTransform,
                        int preInstantiatedElementsCount,
                        bool isAutoExtended)
        {
            RootTransform = rootTransform;
            PreInstantiatedElementsCount = preInstantiatedElementsCount;
            IsAutoExtended = isAutoExtended;
        }

        #endregion
    }
}
