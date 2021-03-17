using System;
using System.Collections.Generic;
using Drawmasters.Levels.Inerfaces;
using Modules.Sound;


namespace Drawmasters.Levels
{
    public class LevelSoundProjectileController : ILevelController, IInitialStateReturn
    {
        #region Fields

        private readonly List<Projectile> projectiles = new List<Projectile>();

        private Guid soundGuid;

        #endregion



        #region Methods

        public void Initialize()
        {
            ProjectileShotComponent.OnShotProduced += ProjectileShotComponent_OnShotProduced;
            ProjectileStayApplyComponent.OnStopProjectile += ProjectileStayApplyComponent_OnStopProjectile;
            ProjectileSmashApplyComponent.OnSmashProjectile += ProjectileSmashApplyComponent_OnSmashProjectile;
        }


        public void Deinitialize()
        {
            ProjectileShotComponent.OnShotProduced -= ProjectileShotComponent_OnShotProduced;
            ProjectileStayApplyComponent.OnStopProjectile -= ProjectileStayApplyComponent_OnStopProjectile;
            ProjectileSmashApplyComponent.OnSmashProjectile -= ProjectileSmashApplyComponent_OnSmashProjectile;

            ClearProjectilesSound();
        }


        private void RefreshSound()
        {
            if (projectiles.IsNullOrEmpty())
            {
                SoundManager.Instance.StopSound(soundGuid);
            }
            else
            {
                if (!SoundManager.Instance.IsActive(soundGuid))
                {
                    soundGuid = SoundManager.Instance.PlaySound(AudioKeys.Ingame.DART_FLYING_LOOP, isLooping: true);
                }
            }
        }


        private void ClearProjectilesSound()
        {
            projectiles.Clear();
            RefreshSound();
            soundGuid = Guid.Empty;
        }


        private void RemoveProjectile(Projectile projectile)
        {
            projectile.OnShouldDestroy -= RemoveProjectile;

            projectiles.Remove(projectile);
            RefreshSound();
        }

        #endregion



        #region Events handlers

        private void ProjectileShotComponent_OnShotProduced(Projectile projectile)
        {
            projectile.OnShouldDestroy += RemoveProjectile;

            projectiles.Add(projectile);
            RefreshSound();
        }


        private void ProjectileStayApplyComponent_OnStopProjectile(Projectile projectile)
        {
            projectile.OnShouldDestroy -= RemoveProjectile;
            RemoveProjectile(projectile);
        }


        private void ProjectileSmashApplyComponent_OnSmashProjectile(Projectile projectile)
        {
            projectile.OnShouldDestroy -= RemoveProjectile;
            RemoveProjectile(projectile);
        }


        public void ReturnToInitialState() =>
            ClearProjectilesSound();

        #endregion
    }
}
