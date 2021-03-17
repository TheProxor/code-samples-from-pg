using Drawmasters.Effects;


namespace Drawmasters.Levels
{
    public abstract class ProjectileReturnToInitialFxComponent : ProjectileComponent
    {
        #region Properties

        protected abstract string FxKey { get; }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        public override void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.ReturnToInitial)
            {
                EffectManager.Instance.PlaySystemOnce(FxKey, 
                    mainProjectile.transform.position, 
                    mainProjectile.transform.rotation);
            }
        }

        #endregion
    }
}
