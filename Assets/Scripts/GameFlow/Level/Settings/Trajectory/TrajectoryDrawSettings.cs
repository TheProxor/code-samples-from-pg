using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "TrajectoryDrawSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "TrajectoryDrawSettings")]
    public class TrajectoryDrawSettings : ScriptableObjectData<TrajectoryDrawSettings.Data, ShooterColorType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<ShooterColorType>
        {
            public Gradient drawLineRendererGradient = default;
            [Tooltip("End line renderer - is reversed this")]
            public Gradient previousLineRendererGradient = default;

            [Enum(typeof(EffectKeys))]
            public string underFingerFxKey = default;
            [Enum(typeof(AudioKeys.Ingame))]
            public string drawSfxKey = default;
        }

        #endregion



        #region Fields

        public int borderRenderersOffsetSegmentsCount = default;
        public int beginSegmentsCount = default;
        public int endSegmentsCount = default;

        public float previousRendererDistance = default;
        public float stayDelaytoStopSound = default;

        [Header("Visual")]
        public LineTextureMode textureMode = default;
        public float lineSegmentSize = default;
        public Material material = default;
        public int cornerVertices = default;
        public FactorAnimation animationEraseLine = default;

        [Header("Width")]
        public float shotRendererWidth = default;
        public float borderShotRendererWidthFinish = default;
        public float borderShotRendererWidthStart = default;


        #endregion



        #region Methods

        public Gradient FindDrawLineRendererGradient(ShooterColorType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.drawLineRendererGradient;
        }
        

        public Gradient FindPreviousLineRendererGradient(ShooterColorType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.previousLineRendererGradient;
        }


        public string FindUnderFingerFxKey(ShooterColorType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.underFingerFxKey;
        }


        public string FindDrawSfxKey(ShooterColorType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.drawSfxKey;
        }

        #endregion
    }
}