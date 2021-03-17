using System.Collections.Generic;
using System.Linq;

namespace Drawmasters.Pool
{
    public class PoolManagerComponentSorter: PoolManagerComponentTemplate, IUpdatable
    {
        #region Fields

        private float initialTime = 0;
        private float watchedTime = 0;

        #endregion



        #region Ctor

        public PoolManagerComponentSorter(float seconds)
        {
            initialTime = seconds;
        }

        #endregion



        #region Private methods

        private void Sort(List<ComponentPool> pools)
        {
            var sorted = pools.OrderByDescending(x => x.Count).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].Data.RootTransform.SetSiblingIndex(i);
            }
        }

        #endregion



        #region IUpdatable

        public void CustomUpdate(float deltaTime)
        {
            if (watchedTime < initialTime)
            {
                watchedTime += deltaTime;
            }
            else
            {
                watchedTime = 0;

                Sort(poolManager.componentPool);
            }
        }

        #endregion
    }
}
