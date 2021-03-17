using Sirenix.OdinInspector;
using UnityEngine;



namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "SniperProjectileSettings",
                   menuName = NamingUtility.MenuItems.IngameSettings + "ModeInfo/SniperProjectileSettings")]
    public class SniperSettings : WeaponSettings, 
        IProjectileLaserDelayDestroy, 
        IProjectileSpeedSettings,
        IProjectileTriggerColliderRadiusSettings,
        IProjectileLineRendererSettings,
        IProjectileImpulsMagnitudeSetting,
        IProjectilePhysicsSettings
    {
        #region Fields

        [Header("Projectile")]
        [MinValue(0.0f)] [SerializeField] float colliderRadius = default;
        [MinValue(0.0f)] [SerializeField] float speed = default;

        [MinValue(0.0f)] public float lifeTime = default;
        [MinValue(0.0f)] public float mass = default;
        [MinValue(0.0f)] [SerializeField] float projectileImpulsMagnitude = default;
        [MinValue(0.0f)] public float gravityScale = default;

        public float additionalOffsetForMonolithEnter = default;

        [Header("Trajectory")]
        [MinValue(0.0f)] [SerializeField] float shotRendererWidth = default;
        [SerializeField] Gradient shotRendererGradient = default;

        [MinValue(0.0f)] public float borderShotRendererWidthStart = default;
        [MinValue(0.0f)] public float borderShotRendererWidthFinish = default;

        public float lineSegmentSize = 0.15f;
        public float smoothDuration = 0.15f;

        [Header("Laser")]
        public float laserDestroyDelay = default;
        public ColorAnimation laserDestroyColorAnimation = default;

        #endregion



        #region Properties

        public override ProjectileType ProjectileType => ProjectileType.Arrow;

        #endregion



        #region Interfaces

        public float LaserDestroyDelay => laserDestroyDelay;

        public ColorAnimation LaserDestroyColorAnimation => laserDestroyColorAnimation;

        #endregion



        #region IProjectileSpeedSetting

        public float Speed => speed;

        public float TriggerColliderRadius => colliderRadius;

        #endregion



        #region IProjectileLineRendererSettings

        public float BeginWidth => shotRendererWidth;

        public float EndWidth => shotRendererWidth;

        public Gradient LineGradient => shotRendererGradient;

        #endregion


        #region IProjectileImpulsMagnitudeSetting

        public float ImpulsMagnitude => projectileImpulsMagnitude;

        #endregion



        #region IProjectilePhysicsSettings

        public float Mass => mass;

        public float GravityScale => gravityScale;

        #endregion
    }
}
