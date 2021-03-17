using UnityEngine;


namespace Drawmasters.Levels
{
    public class PortalgunWeapon : Weapon
    {
        #region Helpers

        public enum ShotState : byte
        {
            None = 0,
            CanShoot = 1,
            Waiting = 2
        }

        #endregion



        #region Fields

        private ShotState portalShotState = ShotState.None;

        private readonly PortalController portalController;

        #endregion



        #region Class lifecycle

        public PortalgunWeapon(WeaponType type,
                            int _projectilesCount,
                            Transform root) :
            base(type, root, _projectilesCount)
        {
            portalShotState = ShotState.CanShoot;

            portalController = new PortalController();
            portalController.Initialize();
        }

        #endregion



        #region Overrided

        public override bool CanShoot => ProjectilesCount > 0;

        public override void Shot(Vector2[] trajectory)
        {
            if (portalShotState == ShotState.CanShoot)
            {
                Vector2 direction = TrajectoryDirection(trajectory);

                Content.Management.CreateProjectile(projectileType,
                                                    Type,
                                                    ShooterColorType.Default,
                                                    trajectory,
                                                    projectilesSpawnRoot);

                portalShotState = ShotState.Waiting;

                TriggerShotCallback(direction);
            }
        }


        public override void SubscribeToEvents()
        {
            ProjectilePortalComponent.OnMonolithCollision += ProjectilePortalComponent_OnMonolithCollision;
            ProjectilePortalComponent.OnProjectileDestroyed += ProjectilePortalComponent_OnProjectileDestroyed;
        }


        public override void UnsubscribeFromEvents()
        {
            ProjectilePortalComponent.OnMonolithCollision -= ProjectilePortalComponent_OnMonolithCollision;
            ProjectilePortalComponent.OnProjectileDestroyed -= ProjectilePortalComponent_OnProjectileDestroyed;
        }


        public override void Deinitialize()
        {
            portalController.Deinitialize();

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void ProjectilePortalComponent_OnMonolithCollision(Vector3 portalPosition, Vector3 leftBorderPosition, Vector3 rightBorderPosition)
        {
            portalController.CreateNewPortal(portalPosition, leftBorderPosition, rightBorderPosition);
        }


        private void ProjectilePortalComponent_OnProjectileDestroyed()
        {
            ProjectilesCount--;

            portalShotState = ShotState.CanShoot;
        }

        

        #endregion
    }
}
