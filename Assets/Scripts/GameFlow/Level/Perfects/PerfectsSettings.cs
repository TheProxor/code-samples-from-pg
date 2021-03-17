using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "PerfectsSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "PerfectsSettings")]
    public class PerfectsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            public PerfectType type = default;
            public float minDelayForNextReceive = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Data[] data = default; // TODO: move into perfect/announcer settings (here cuz of lc changes)

        public float killEnemyReward = default;
        public float levelCompleteReward = default;

        #endregion



        #region Methods

        public float GetMinDelayForNextReceive(PerfectType type)
        {
            if (type == PerfectType.None)
            {
                return default;
            }

            Data foundData = Array.Find(data, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"Do data found for type {type} in {this}");
                return default;
            }

            return foundData.minDelayForNextReceive;
        }

        #endregion
    }
}
