using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [Serializable]
    public class ScriptableObjectBaseData<K>
    {
        public K key = default;
    };

    public abstract class ScriptableObjectData<D, K> : ScriptableObject where D : ScriptableObjectBaseData<K>
    {
        #region Fields

        [SerializeField] protected D[] data = default;

        #endregion



        #region Methods

        protected D FindData(K type)
        {
            D foundData = Array.Find(data, e => type.Equals(e.key));

            if (foundData == null)
            {
                CustomDebug.LogWarning($"No data found for {type} in {this}");
            }

            return foundData;
        }


        protected void AssertLog(bool assertCondition, string log)
        {
            if (assertCondition)
            {
                CustomDebug.Log(log);
            }
        }

        #endregion
    }
}