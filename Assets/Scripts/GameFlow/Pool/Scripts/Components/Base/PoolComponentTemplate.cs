using UnityEngine;
using System.Collections;

namespace Drawmasters.Pool
{
    public class PoolManagerComponentTemplate
    {
        #region Fields

        protected PoolManager poolManager;

        #endregion



        #region Methods

        public virtual void Initialize(PoolManager _poolManager)
        {
            poolManager = _poolManager;
        }

        #endregion
    }
}
