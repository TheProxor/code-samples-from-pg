using UnityEngine;
using Drawmasters.Effects;
using Modules.Sound;
using Drawmasters.Vibration;
using System;


namespace Drawmasters.Levels
{
    public abstract class ProjectileStayComponent : ProjectileSettingsComponent
    {
        #region Fields

        public static event Action<Projectile> OnProjectileStay;

        private readonly CollisionNotifier projectileCollisionNotifier;

        protected float offset;

        #endregion



        #region Properties

        protected abstract bool CanStayProjectileOnCollision(CollidableObject collidableObject, out Vector3 stayPosition);

        protected abstract string FxKeyOnStop { get; }
        protected abstract string SfxKeyOnStop { get; }

        #endregion



        #region Lifecycle

        public ProjectileStayComponent(CollisionNotifier _projectileCollisionNotifier)
        {
            projectileCollisionNotifier = _projectileCollisionNotifier;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            projectileCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;

            mainProjectile.MainRigidbody2D.simulated = true;
        }


        public override void Deinitialize()
        {
            projectileCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (CanStayProjectileOnCollision(collidableObject, out Vector3 stayPosition))
            {
                mainProjectile.MainRigidbody2D.simulated = false;

                EffectManager.Instance.PlaySystemOnce(FxKeyOnStop,
                                      mainProjectile.transform.position,
                                      mainProjectile.transform.rotation);
                SoundManager.Instance.PlaySound(SfxKeyOnStop);
                VibrationManager.Play(IngameVibrationType.Ricochet);

                mainProjectile.StopTrajectoryPath();

                mainProjectile.transform.position = stayPosition;

                OnProjectileStay?.Invoke(mainProjectile);
            }
        }


        protected override void ApplySettings(WeaponSettings settings)
        {
            if (settings is SniperSettings sniperSettings)
            {
                offset = sniperSettings.additionalOffsetForMonolithEnter;
            }
        }
    }

    #endregion
}
