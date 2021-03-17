using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "LevelSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LevelSettings")]
    public class CommonLevelSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class GamePlayDelayData
        {
            public GameMode mode = default;
            public float delay = default;
        }

        #endregion



        #region Fields

        [SerializeField] private GamePlayDelayData[] gamePlayDelayData = default;

        public float loseEndDelay = default;
        public float allTargetHittedEndDelay = default;

        public float allowShotDelay = default;

        public float bonusLevelEndDelay = default;
        public float bossLevelEndDelay = default;

        public Bounds levelFieldBounds = default;

        public float GetGameplayDelay(GameMode mode)
        {
            GamePlayDelayData foundData = Array.Find(gamePlayDelayData, e => e.mode == mode);
            return foundData == null ? default : foundData.delay;
        }

        #endregion
    }
}
