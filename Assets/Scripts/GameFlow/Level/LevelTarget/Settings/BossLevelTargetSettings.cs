using System;
using UnityEngine;
using Spine.Unity;
using Drawmasters.Levels;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "BossLevelTargetSettings",
                   menuName = NamingUtility.MenuItems.IngameSettings + "BossLevelTargetSettings")]
    public class BossLevelTargetSettings : ScriptableObjectData<BossLevelTargetSettings.Data, ShooterColorType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<ShooterColorType>
        {
            public Sprite visualSprite = default;
            public string trailFxKey = default;
        }

        #endregion



        #region Fields
#pragma warning disable 0414

        [Tooltip("only for reflection")]
        [SerializeField] private SkeletonDataAsset asset = default;

#pragma warning restore

        [SpineSkin(dataField = "asset")] public string[] skins = default;
        [SpineSkin(dataField = "asset")] public string skinWithBoundingBoxes = default;

        public float firstComeDuration = default;
        public float firstComeDelay = default;

        [Header("Come animation")]
        public AnimationCurve comeCurve = default;
        public float comeDuration = default;
        public float comeDelay = default;

        [Header("Leave animation")]
        public AnimationCurve leaveCurve = default;
        public float leaveDuration = default;
        public float leaveDelay = default;

        [Header("Trajectory draw")]
        public float drawTrajectoriesFirstDelay = default;
        public float drawTrajectoriesDelay = default;
        public float drawTrajectoriesTimescaleMultiply = default;

        [Header("Defeat")]
        public float defeatSoundDelay = default;
        public float[] defeatExplodeSoundsDelay = default;

        public float startDefeatDelay = default;
        public float explosionDelay = default;
        public float defeatDelay = default;
        public ColorAnimation defeatColorAnimation = default;

        [Tooltip("Begin and end values set up from code")]
        public VectorAnimation defeatMoveToCenterAnimation = default;
        public float defeatMoveToCenterDistance = default;


        [Header("Hitmasters settings")]
        public ColorAnimation bossFadeAnimation = default;
        public ColorAnimation bossAppearAnimation = default;

#pragma warning disable 0414

        [Tooltip("only for reflection")]
        [SerializeField] private SkeletonDataAsset hitmastersBossAsset = default;

#pragma warning restore

        [SpineSkin(dataField = "hitmastersBossAsset")] public string[] hitmastersBossSkins = default;
        [SpineSkin(dataField = "hitmastersBossAsset")] public string hitmastersBossSkinWithoutBoundingBoxes = default;

        #endregion



        #region Methods

        public Sprite FindVisualSprite(ShooterColorType colorType)
        {
            Data foundData = FindData(colorType);
            return foundData == null ? default : foundData.visualSprite;
        }


        public string FindTrailFxKey(ShooterColorType colorType)
        {
            Data foundData = FindData(colorType);
            return foundData == null ? default : foundData.trailFxKey;
        }
        
        #endregion
    }
}
