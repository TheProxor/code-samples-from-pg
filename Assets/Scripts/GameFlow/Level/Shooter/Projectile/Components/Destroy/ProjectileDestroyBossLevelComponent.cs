using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ProjectileDestroyBossLevelComponent : ProjectileComponent
    {
        #region Fields

        private readonly LevelContext context;

        #endregion



        #region Class lifecycle

        public ProjectileDestroyBossLevelComponent()
        {
            context = GameServices.Instance.LevelEnvironment.Context;
        }

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
            if (context.IsBossLevel && state == LevelState.FriendlyDeath)
            {
                mainProjectile.StartImmediatelyLaserDestroy();
            }
        }

        #endregion
    }
}
