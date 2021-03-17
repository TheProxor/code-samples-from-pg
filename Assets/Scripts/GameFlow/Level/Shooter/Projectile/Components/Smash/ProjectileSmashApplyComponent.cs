using System;
using DG.Tweening;


namespace Drawmasters.Levels
{
    public class ProjectileSmashApplyComponent : ProjectileComponent
    {
        #region Fields

        public static event Action<Projectile> OnSmashProjectile;

        #endregion



        #region Properties

        protected CollidableObjectType[] CollidableObjectTypes { get; }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainProjectile.OnShouldSmash += SmashWithCollidableObject;
        }


        public override void Deinitialize()
        {
            mainProjectile.OnShouldSmash -= SmashWithCollidableObject;

            DOTween.Kill(mainProjectile);

            base.Deinitialize();
        }


        private void SmashWithCollidableObject(CollidableObject collidableObject)
        {
            if (collidableObject != null)
            {
                OnSmashProjectile?.Invoke(mainProjectile);

                PhysicsSolutions.ApplySmashSettings(ref mainProjectile, collidableObject.transform.position, collidableObject.Type);
            }
        }

        #endregion
    }
}
