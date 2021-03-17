using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "AnnouncerSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "AnnouncerSettings")]
    public class AnnouncerSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class PerfectsData
        {
            public PerfectType type = default;
            public Sprite sprite = default;
        }

        #endregion



        #region Fields

        public Announcer announcerPrefab = default;
        public float announcersMinDelay = default;

        [Header("Level settings")]
        public float outOfAmmoDelay = default;

        [Header("Visual settings")]
        [SerializeField] private PerfectsData[] visualData = default;

        public float offsetY = default;

        public float lifeTime = default;

        [Header("Pefects curves")]
        public AnimationCurve moveCurve = default;
        public AnimationCurve alphaCurve = default;

        [Header("Ingame screen announcers")]
        public VectorAnimation commonAnnouncer = default;
        public VectorAnimation bonusLevelAnnouncer = default;
        public VectorAnimation bossLevelAnnouncer = default;
        public VectorAnimation bossDefeatAnnouncer = default;
        public VectorAnimation bonusLevelStartDrawAnnouncer = default;
        public VectorAnimation bonusLevelFinishedAnnouncer = default;

        public VectorAnimation fastHideAnimation = default; 

        #endregion



        #region Methods

        public Sprite GetSprite(PerfectType type)
        {
            PerfectsData foundData = Array.Find(visualData, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"Do data found for type {type} in {this}");
                return null;
            }

            return foundData.sprite;
        }

        #endregion
    }
}
