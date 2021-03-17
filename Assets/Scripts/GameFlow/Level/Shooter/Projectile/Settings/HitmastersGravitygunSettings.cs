using Sirenix.OdinInspector;
using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "HitmastersGravitygunSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "ModeInfo/HitmastersGravitygunSettings")]
    public class HitmastersGravitygunSettings : WeaponSettings,
        IProjectileLineRendererSettings,
        IProjectileSpeedSettings
    {
        #region Nested types

        [Serializable]
        private class LimbsPullData
        {
            public string limbsBoneName = default;
            public float pullMultiplier = default;
        }

        #endregion



        #region Fields

        [Header("Throw")]
        [Range(0.0f, 1.0f)]
        [MinValue(0.0f)] public float minDistanceFactorForThrowHit = default;
        [Tooltip("для всех объектов")]
        [MinValue(0.0f)] public float throwImpulsMagnitudeObjects = default;
        [Tooltip("живой Level Target")]
        [MinValue(0.0f)] public float throwImpulsMagnitudeLevelTarget = default;
        [Tooltip("для оторваных конечностей")]
        [MinValue(0.0f)] public float throwImpulsMagnitudeLimbs = default;

        [MinValue(0.0f)] public float fallImpulsMagnitude = default;
        public FactorAnimation gravityReturnAnimation = default;

        [Header("Projectile speed")]
        [SerializeField] private float projectileSpeed = default;

        [Header("Pull")]
        [Tooltip("Скорость притяжения для объект рассчитывается как pullSpeedForOneMassUnit / массу объекта")]
        [MinValue(0.0f)] public float pullSpeedForOneMassUnit = default;
        [MinValue(0.0f)] public float rootCharacterPullMultiplier = default;
        [Tooltip("Оффсет от кости пушки")]
        [MinValue(0.0f)] public float distanceToStopPull = default;

        [Tooltip("Если во время притяжения в след кадре будет различие больше чем на этот угол, то перестанем притягивать")]
        [MinValue(0.0f)] public float anglePerFrameToStopPull = default;

        [Tooltip("Если во время того, как у нас объект притянут, угол между пушкой и объектом будет больше, чем такой, то связь разорвется")]
        [MinValue(0.0f)] public float anglePerFrameToReset = default;

        [Tooltip("Максимальный угол для объектов чтобы сброситься")]
        [MinValue(0.0f)] public float maxObjectsAngleToReset = default;
        [MinValue(0.0f)] public float maxObjectsDistanceToReset = default;

        [Header("Trajectory")]
        [MinValue(0.0f)] public float shotRendererWidth = default;
        [SerializeField] private Gradient shotRendererGradient = default;

        public float additionalRendererDistanceOnHit = default;

        [Header("LevelTarget")]
        [MinValue(0.0f)] public float disableCollisionDurationAfterPull = default;
        [SerializeField] private LimbsPullData[] limbsPullData = default;

        #endregion



        #region Abstract implementation

        public override ProjectileType ProjectileType => ProjectileType.HitmastersGravitygunBullet;


        #endregion



        #region IProjectileLineRendererSettings

        public float BeginWidth => shotRendererWidth;

        public float EndWidth => shotRendererWidth;

        public Gradient LineGradient => shotRendererGradient;

        #endregion


        #region IProjectileSpeedSettings

        public float Speed => projectileSpeed;

        #endregion



        #region Public methods

        public bool AllowPullBodyLimb(string boneName) 
            => Array.Exists(limbsPullData, e => e.limbsBoneName == boneName);


        public float LimbPullMultiplierMagnitude(string boneName)
        {
            LimbsPullData foundData = Array.Find(limbsPullData, e => e.limbsBoneName == boneName);
            return (foundData == null) ? default : foundData.pullMultiplier;
        }

        #endregion
    }
}
