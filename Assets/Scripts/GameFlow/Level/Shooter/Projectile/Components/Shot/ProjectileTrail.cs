using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ProjectileTrail : ProjectileComponent
    {
        #region Fields

        private string trailName;
        private EffectHandler effectHandler;

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainProjectile.OnShouldShot += MainProjectile_OnShouldShot;

            trailName = GetTrailFxKey();
        }


        public override void Deinitialize()
        {
            mainProjectile.OnShouldShot -= MainProjectile_OnShouldShot;

            if (!effectHandler.IsNull())
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }

            base.Deinitialize();
        }


        protected abstract string GetTrailFxKey();


        #endregion


        #region Events handlers

        void MainProjectile_OnShouldShot(Vector2[] v)
        {
            if (!string.IsNullOrEmpty(trailName))
            {
                effectHandler = EffectManager.Instance.CreateSystem(trailName,
                                                                    true,
                                                                    mainProjectile.transform.position,
                                                                    default,
                                                                    mainProjectile.Root,
                                                                    TransformMode.World,
                                                                    false);
                if (effectHandler != null)
                {
                    effectHandler.Play(withClear : false);
                }
            }
        }

        #endregion
    }
}
