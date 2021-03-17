using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CommonSpikesSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "CommonSpikesSettings")]
    public class CommonSpikesSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            public PhysicalLevelObjectData data = default;
            public int spriteIndex = default;
            public Sprite leftBorderDecalSprite = default;
            public Sprite middleDecalSprite = default;
            public Sprite rightBorderDecalSprite = default;
        }

        #endregion



        #region Fields

        public Data[] visualData = default;

        public FactorAnimation alphaDecalAnimation = default;

        public SpriteRenderer decalSpriteRenderer = default;

        public float delayForLevelTargetFix = default;

        [SerializeField] private string[] allowSlowLimbs = default;

        #endregion



        #region Methods

        public bool AllowSlowLimb(string boneName) => Array.Exists(allowSlowLimbs, e => e == boneName);

        #endregion
    }
}
