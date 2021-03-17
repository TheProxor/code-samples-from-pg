using Sirenix.OdinInspector;
using UnityEngine;



namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "HitmastersShotgunSettings",
                   menuName = NamingUtility.MenuItems.IngameSettings + "ModeInfo/HitmastersShotgunSettings")]
    public class HitmastersShotgunSettings : WeaponSettings,
        IProjectileImpulsMagnitudeSetting,
        IProjectileSpeedSettings,
        IProjectilePhysicsSettings,
        IProjectileTriggerColliderRadiusSettings,
        IProjectilePhysicsColliderRadiusSettings,
        IProjectileLineRendererSettings
    {
        #region Fields

        [Header("Projectile")]
        [MinValue(0.0f)] public float colliderRadius = default;
        [MinValue(0.0f)] [SerializeField] float speed = default;
        [MinValue(0.0f)] public float lifeTime = default;
        [MinValue(0.0f)] [SerializeField] private float mass = default;
        [MinValue(0.0f)] [SerializeField] private float gravityScale = default;

        [Header("Weapon")]
        [MinValue(1)] public int projectilesCountPerShot = default;

        [MinValue(0.1f)] public float spawnWidth = default;

        [MinValue(0.0f)] public float maxImpulsPerShot = default;
        [DisableInEditorMode] [SerializeField] float projectileImpulsMagnitude = default;

        [Header("Trajectory")]
        [MinValue(0.0f)] public float shotRendererWidth = default;
        public Gradient shotRendererGradient = default;

        #endregion



        #region Properties

        public override ProjectileType ProjectileType => ProjectileType.HitmastersShotgunBullet;


        #endregion



        #region IProjectileImpulsMagnitudeSetting

        public float ImpulsMagnitude => projectileImpulsMagnitude;

        #endregion



        #region IProjectileSpeedSettings

        public float Speed => speed;

        #endregion



        #region IProjectilePhysicsSettings

        public float Mass => mass;

        public float GravityScale => gravityScale;

        #endregion



        #region IProjectileTriggerColliderRadiusSettings

        public float TriggerColliderRadius => colliderRadius;

        #endregion



        #region IProjectileLineRendererSettings

        public float BeginWidth => shotRendererWidth;

        public float EndWidth => shotRendererWidth;

        public Gradient LineGradient => shotRendererGradient;

        #endregion



        #region IProjectilePhysicsColliderRadiusSettings

        public float PhysicsColliderRadius => colliderRadius;

        #endregion






        #region Editor methods

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (projectilesCountPerShot == 0)
            {
                projectileImpulsMagnitude = default;
                return;
            }

            projectileImpulsMagnitude =  maxImpulsPerShot / projectilesCountPerShot;
        }

        #endif

        #endregion
    }
}
