using System;
using System.Collections.Generic;
using UnityEngine;
using Drawmasters.Vibration;
using Drawmasters.Effects;

namespace Drawmasters.Levels
{
    public class ProjectileReboundComponent : ProjectileComponent
    {
        #region Fields

        public static event Action<Projectile> OnRebound;

        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private List<CollidableObjectType> typesToRebound = default;

        #endregion



        #region Lifecycle

        public ProjectileReboundComponent(CollisionNotifier _collisionNotifier, List<CollidableObjectType> _typesToRebound)
        {
            collisionNotifier = _collisionNotifier;
            typesToRebound = _typesToRebound;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
        }


        public override void Deinitialize()
        {
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject reference, Collision2D collision)
        {
            CollidableObject collidedObject = collision.gameObject.GetComponent<CollidableObject>();

            if (collidedObject != null &&
                typesToRebound.Contains(collidedObject.Type))
            {
                OnRebound?.Invoke(mainProjectile);

                Vector3 vfxPosition = (collision.contactCount > 0) ?
                                       collision.GetContact(0).point.ToVector3() : reference.transform.position;

                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxRicochetBullet, 
                                                      vfxPosition, 
                                                      Quaternion.identity, 
                                                      null, 
                                                      TransformMode.World);

                VibrationManager.Play(IngameVibrationType.Ricochet);
            }
        }

        #endregion
    }
}
