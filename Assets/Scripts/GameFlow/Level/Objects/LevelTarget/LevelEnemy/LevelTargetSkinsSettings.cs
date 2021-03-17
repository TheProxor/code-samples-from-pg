using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "LevelTargetSkinsSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LevelTargetSkinsSettings")]
    public partial class LevelTargetSkinsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class ColorsData : BaseColorsData
        {
            public Color color = default;
        }


        [Serializable]
        public class LevelTargetMaterialsColorsData : ScriptableObjectBaseData<SkeletonDataAsset>
        {
            public Material originalMaterial = default;
            public Material outlineMaterial = default;
        }

        #endregion



        #region Fields

        [SerializeField] private ColorsData[] colorsData = default;
        [SerializeField] private ColorsData[] bossColorsData = default;
        public SkinsColorsData[] skinsData = default;
        public LevelTargetMaterialsColorsData[] materialsColorsData = default;

        public float outlineResetOnShotDelay = default;

        public string skinWithBoundingBoxes = default;

        public FactorAnimation thresholdAnimation = default;


        #endregion



        #region Methods

        public Material FindOriginalMaterial(SkeletonDataAsset skeletonDataAsset)
        {
            LevelTargetMaterialsColorsData data = Array.Find(materialsColorsData, e => e.key == skeletonDataAsset);
            return data == null ? default : data.originalMaterial;
        }


        public Material FindOutlineMaterial(SkeletonDataAsset skeletonDataAsset)
        {
            LevelTargetMaterialsColorsData data = Array.Find(materialsColorsData, e => e.key == skeletonDataAsset);
            return data == null ? default : data.outlineMaterial;
        }


        public Color FindBossOutlineColor(ShooterColorType colorType)
        {
            ColorsData data = Array.Find(bossColorsData, e => e.key == colorType);
            return data == null ? default : data.color;
        }


        public Color FindOutlineColor(ShooterColorType colorType)
        {
            ColorsData data = Array.Find(colorsData, e => e.key == colorType);
            return data == null ? default : data.color;
        }

        public string FindColorSkin(ShooterColorType colorType)
        {
            SkinsColorsData data = Array.Find(skinsData, e => e.key == colorType);
            return data == null ? string.Empty : data.skinName;
        }

        #endregion
    }
}
