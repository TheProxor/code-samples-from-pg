using Sirenix.OdinInspector;
using UnityEngine;

namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "HitmastersSniperSettings",
                       menuName = NamingUtility.MenuItems.IngameSettings + "ModeInfo/HitmastersSniperSettings")]
    public class HitmastersSniperSettings : WeaponSettings, 
        IProjectileLaserDelayDestroy, 
        IProjectileSpeedSettings,
        IProjectileLineRendererSettings,
        IProjectileImpulsMagnitudeSetting,
        IProjectilePhysicsSettings,
        IProjectilePhysicsMaterialSettings,
        IProjectileTriggerColliderRadiusSettings,
        IProjectilePhysicsColliderRadiusSettings
    {
        #region Abstract implementation

        public override ProjectileType ProjectileType => ProjectileType.HitmastersSniperBullet;

        #endregion



        #region Fields

        [Header("Projectile")]
        [MinValue(0.0f)] [SerializeField] float colliderRadius = default;
        [MinValue(0.0f)] [SerializeField] float speed = default;
        [MinValue(0.0f)] public float lifeTime = default;
        [MinValue(0.0f)] [SerializeField] float mass = default;
        [MinValue(0.0f)] [SerializeField] float projectileImpulsMagnitude = default;
        [MinValue(0.0f)] [SerializeField] float gravityScale = default;

        public float additionalOffsetForMonolithEnter = default;

        [Header("Trajectory")]
        [MinValue(0.0f)] public float shotRendererWidth = default;

        public Gradient shotRendererGradient = default;

        [MinValue(0.0f)] public float borderShotRendererWidthStart = default;
        [MinValue(0.0f)] public float borderShotRendererWidthFinish = default;

        public float lineSegmentSize = 0.15f;
        public float smoothDuration = 0.15f;

        [Header("Laser")]
        public float laserDestroyDelay = default;
        public ColorAnimation laserDestroyColorAnimation = default;

        [Header("Physics material settings")]
        [Range(0f, 1f)] [SerializeField] private float friction = default;
        [Range(0f, 1f)] [SerializeField] private float bonciness = default;

        #endregion


        #region Interfaces

        public float LaserDestroyDelay => laserDestroyDelay;

        public ColorAnimation LaserDestroyColorAnimation => laserDestroyColorAnimation;


        #endregion



        #region IProjectileSpeedSetting

        public float Speed => speed;

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



        #region IProjectilePhysicsMaterialSettings

        public float Friction => friction;

        public float Bounciness => bonciness;

        #endregion



        #region IProjectileTriggerColliderRadiusSettings

        public float TriggerColliderRadius => colliderRadius;

        #endregion



        #region IProjectilePhysicsColliderRadiusSettings

        public float PhysicsColliderRadius => colliderRadius;

        #endregion
    }
}
