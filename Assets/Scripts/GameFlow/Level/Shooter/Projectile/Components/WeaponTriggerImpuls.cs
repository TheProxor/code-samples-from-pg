using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class WeaponTriggerImpuls : ProjectileSettingsComponent
    {
        #region Fields
        
        public static event Action<string> OnShouldLog;

        [SerializeField] private CollisionNotifier collisionNotifier = default;

        protected float impulsMagnitude;

        #endregion



        #region Lifecycle

        public WeaponTriggerImpuls(CollisionNotifier _collisionNotifier)
        {
            collisionNotifier = _collisionNotifier;
        }

        #endregion



        #region Public methods

        public sealed override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
        }


        public sealed override void Deinitialize()
        {
            collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;

            base.Deinitialize();
        }

        #endregion



        #region Private methods

        private void HandlePhysicalLevelObjectCollision(GameObject physicalGameObject)
        {
            PhysicalLevelObject physicalLevelObject = physicalGameObject.GetComponent<PhysicalLevelObject>();

            if (physicalLevelObject == null)
            {
                return;
            }

            bool canTakeCollistion = CanTakeCollision(physicalLevelObject);
            if (!canTakeCollistion)
            {
                return;
            }

            Vector2 impulsToAdd = PhysicsCalculation.GetCollisionImpulsForObject(mainProjectile, impulsMagnitude);
            physicalLevelObject.AddImpuls(mainProjectile.PreviousFrameRigidbody2D.Position, impulsToAdd);

            OnShouldLog?.Invoke($"Projectile hit {physicalLevelObject.name}. Impuls to add {impulsToAdd.magnitude}");        
        }


        protected abstract bool CanTakeCollision(PhysicalLevelObject physicalLevelObject);

        protected override void ApplySettings(WeaponSettings weaponSettings)
        {
            if (weaponSettings is IProjectileImpulsMagnitudeSetting projectileMagnitudeSettings)
            {
                impulsMagnitude = projectileMagnitudeSettings.ImpulsMagnitude;
            }
            else
            {
                LogError(weaponSettings.GetType().Name, nameof(IProjectileImpulsMagnitudeSetting));
            }
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            HandlePhysicalLevelObjectCollision(collision.gameObject);
        }

        #endregion
    }
}