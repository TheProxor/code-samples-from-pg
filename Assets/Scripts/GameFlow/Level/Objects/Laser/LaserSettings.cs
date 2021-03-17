using UnityEngine;
using System;
using Drawmasters.Utils;

namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "LaserSettings",
                    menuName = NamingUtility.MenuItems.IngameSettings + "LaserSettings")]
    public class LaserSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class PhysicalObjectEffectsData
        {
            public PhysicalLevelObjectSizeType objectSizeType = default;
            public PhysicalLevelObjectShapeType objectShapeType = default;

            [Enum(typeof(EffectKeys))] public string destroyEffectKey = default;
            [Enum(typeof(EffectKeys))] public string destroyEndEffectKey = default;
        }

        #endregion



        #region Fields

        public float defaultRayDistance = default;
        public float shotRendererWidth = default;
        public float collisionFxOffset = default;

        [Header("Level target")]
        public float levelTargetDestoyDelay = default;

        [Header("Visual")]
        public Color laserHittedEndColor = Color.black;

        [Header("Effects")]
        public LineFx.Settings startLineFxSettings = default;
        public LineFx.Settings rayLineFxSettings = default;


        [SerializeField] private PhysicalObjectEffectsData[] effectsData = default;

        #endregion



        #region Methods

        public string GetDestroyEffect(PhysicalLevelObjectData physicalData)
        {
            PhysicalObjectEffectsData effectsData = FindObjectsEffectData(physicalData);
            return effectsData == null ? string.Empty : effectsData.destroyEffectKey;
        }


        public string GetDestroyEndEffect(PhysicalLevelObjectData physicalData)
        {
            PhysicalObjectEffectsData effectsData = FindObjectsEffectData(physicalData);
            return effectsData == null ? string.Empty : effectsData.destroyEndEffectKey;
        }


        private PhysicalObjectEffectsData FindObjectsEffectData(PhysicalLevelObjectData physicalData)
        {
            PhysicalObjectEffectsData foundData = Array.Find(effectsData, e => e.objectShapeType == physicalData.shapeType &&
                                                                                  e.objectSizeType == physicalData.sizeType);

            if (foundData == null)
            {
                CustomDebug.Log($"No data found for {physicalData.shapeType} and {physicalData.sizeType} in {this}");
            }

            return foundData;
        }

        #endregion
    }
}
