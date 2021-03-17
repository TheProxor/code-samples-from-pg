using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LiquidProjectileEffectComponent : LiquidGraphicVisualComponent
    {
        #region Methods

        public override void Enable()
        {
            base.Enable();

            liquid.CollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
        }


        public override void Disable()
        {
            liquid.CollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject obj, Collider2D collider2D)
        {
            CollidableObject collidableObject = collider2D.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            Projectile projectile = collidableObject.Projectile;

            if (projectile != null)
            {
                EffectManager.Instance.PlaySystemOnce(visualSettings.projectileCollisionEffectKey, projectile.PreviousFrameRigidbody2D.Position);
            }
        }

        #endregion
    }
}
