using Drawmasters.ServiceUtil;
using Modules.Sound;
using System;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class GravitygunWeapon : Weapon
    {
        #region Helpers

        private enum ShotState : byte
        {
            None = 0,
            Idle = 1,
            Pulling = 2
        }

        #endregion


        #region Fields

        public static event Action<Transform, Vector2> OnShouldPullObject; // projectilespawn direction
        public static event Action<Vector2> OnShouldThrowObject; // direction

        private ShotState shotState;
        Guid soundGuid;

        #endregion



        #region Lifecycle

        public GravitygunWeapon(WeaponType type, int _projectileCount, Transform projectileSpawnRoot)
            : base(type, projectileSpawnRoot, _projectileCount)
        {
            shotState = ShotState.Idle;
        }

        #endregion



        #region Overrided

        public override bool CanShoot => ProjectilesCount > 0;

        public override void Deinitialize()
        {
            SoundManager.Instance.StopSound(soundGuid);

            base.Deinitialize();
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            ProjectilePullThrowComponent.OnPullMiss += ProjectilePullThrowComponent_OnPullMiss;
            Shooter.OnStartAiming += Shooter_OnStartAiming;
        }


        public override void UnsubscribeFromEvents()
        {
            ProjectilePullThrowComponent.OnPullMiss -= ProjectilePullThrowComponent_OnPullMiss;
            Shooter.OnStartAiming -= Shooter_OnStartAiming;

            base.UnsubscribeFromEvents();
        }


        public override void Shot(Vector2[] trajectory)
        {
            Vector2 direction = TrajectoryDirection(trajectory);

            switch (shotState)
            {
                case ShotState.Idle:
                    shotState = ShotState.Pulling;
                    Content.Management.CreateProjectile(projectileType,
                                                        Type, 
                                                        ShooterColorType.Default,
                                                        trajectory, 
                                                        projectilesSpawnRoot);
                    OnShouldPullObject?.Invoke(projectilesSpawnRoot, direction);
                    break;

                case ShotState.Pulling:                    
                    ProjectilesCount--;
                    OnShouldThrowObject?.Invoke(direction);
                    shotState = ShotState.Idle;
                    break;

                default:
                    CustomDebug.Log($"Logic in {this} dont implemented for state {shotState}");
                    break;
            }

            TriggerShotCallback(direction);

            SoundManager.Instance.StopSound(soundGuid);
        }
        
        #endregion



        #region Events handlers

        private void ProjectilePullThrowComponent_OnPullMiss()
        {
            ProjectilesCount--;

            shotState = ShotState.Idle;
        }


        private void Shooter_OnStartAiming()
        {
            if (shotState == ShotState.Idle)
            {
                SoundManager.Instance.StopSound(soundGuid);
                
                soundGuid = SoundManager.Instance.PlaySound(AudioKeys.Ingame.GRAVYGUNBEAMLOOP, isLooping: true);
            }
        }        

        #endregion
    }
}
