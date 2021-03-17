using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "LevelGraphicSettings",
                         menuName = NamingUtility.MenuItems.IngameSettings + "LevelGraphicSettings")]
    public class LevelGraphicSettings : ScriptableObject
    {
        #region Helper types

        [Serializable]
        public class Data
        {
            public GameMode mode;
            public int chapterIndex;
            public int colorProfileIndex;
        }

        #endregion



        #region Fields

        [FormerlySerializedAs("usualData")] [Header("Main data")]
        public List<Data> graphicsData = default;

        [SerializeField] private int[] rewardDataProposalForcemeterIndexes = default;

        #endregion



        #region Properties

        public int RandomForcemeterRewardIndex => rewardDataProposalForcemeterIndexes.RandomObject();

        #endregion
    }
}