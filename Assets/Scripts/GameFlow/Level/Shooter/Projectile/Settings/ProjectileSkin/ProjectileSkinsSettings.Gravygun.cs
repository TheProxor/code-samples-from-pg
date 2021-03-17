using System;
using UnityEngine;

namespace Drawmasters.Levels
{
    public partial class ProjectileSkinsSettings
    {
        #region Helper types

        [Serializable]
        private class PullEffectsData
        {            
            public ShapeTypeEffectsData[] shapeTypeEffectsData = default;
        }

        [Serializable]
        private class ShapeTypeEffectsData
        {
            public PhysicalLevelObjectShapeType objectShapeType = default;
            public PhysicalLevelObjectSizeType objectSizeType = default;

            [Enum(typeof(EffectKeys))] public string startEffectKey = default;
            [Enum(typeof(EffectKeys))] public string effectKey = default;

            public Vector3 effectsScale = Vector3.one;
        }

        #endregion



        #region Fields

        [SerializeField] private PullEffectsData pullEffectsData = default;

        #endregion



        #region Public methods

        public string GetPullStartEffectKey(PhysicalLevelObjectData data)
        {
            ShapeTypeEffectsData foundData = FindPullEffectsData(data);

            return foundData != null ? foundData.startEffectKey : string.Empty;
        }

        public string GetPullEffectKey(PhysicalLevelObjectData data)
        {
            ShapeTypeEffectsData foundData = FindPullEffectsData(data);

            return foundData != null ? foundData.effectKey : string.Empty;
        }


        public Vector3 GetPullEffectScaleKey(PhysicalLevelObjectData data)
        {
            ShapeTypeEffectsData foundData = FindPullEffectsData(data);

            return foundData != null ? foundData.effectsScale : Vector3.one;
        }

        #endregion



        #region Private methods

        private ShapeTypeEffectsData FindPullEffectsData(PhysicalLevelObjectData objectData)
        {
            ShapeTypeEffectsData effectsData = pullEffectsData.shapeTypeEffectsData.Find(e => e.objectShapeType == objectData.shapeType &&
                                                                                              e.objectSizeType == objectData.sizeType);
            if (effectsData == null)
            {
                CustomDebug.Log($"Cannot find {nameof(ShapeTypeEffectsData)}. For data : {objectData.shapeType}, {objectData.sizeType}, {objectData.type}");
            }

            return effectsData;
        }

        #endregion
    }


}

