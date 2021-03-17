using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public abstract class ProjectileHitHandler : LimbsCollisionHandler
    {
        #region Fields

        protected List<Projectile> hittedProjectiles;

        #endregion


        #region Lifecycle

        public ProjectileHitHandler(List<LevelTargetLimb> _enemyLimbs) : base(_enemyLimbs)
        {

        }

        #endregion



        #region Abtract methods

        protected abstract void HandleSpikesCollision(Spikes spikes, LevelTargetLimb limb, out float damage);


        protected abstract void HandleLiquidCollision(LiquidLevelObject liquid, LevelTargetLimb limb, out float damage);


        protected abstract void HandleMonolithCollision(LevelTargetLimb limb, out float damage);


        protected abstract void HandleAnotherLimbPartCollision(LevelTargetLimbPart anotherPart,
                                                              LevelTarget anotherLevelTarget,
                                                              LevelTargetLimb currentTargetLimb,
                                                              out float damage);


        protected abstract void HandlePhysicalLevelObjectCollision(PhysicalLevelObject physicalLevelObject,
                                                                  LevelTargetLimb limb,
                                                                  out float damage);

        #endregion



        #region Methods

        public override void Initialize(LevelTarget _levelTarget)
        {
            base.Initialize(_levelTarget);

            // Not readonly cuz of Odin serialization.
            hittedProjectiles = new List<Projectile>();
        }

        public override void Enable()
        {
            base.Enable();

            ProjectileReboundComponent.OnRebound += ProjectileReboundComponent_OnRebound;
        }


        public override void Disable()
        {
            ProjectileReboundComponent.OnRebound -= ProjectileReboundComponent_OnRebound;

            foreach (var projectile in hittedProjectiles)
            {
                projectile.OnShouldDestroy -= Projectile_OnShouldDestroy;
            }

            hittedProjectiles.Clear();

            base.Disable();
        }


        protected virtual bool HandleProjectileCollision(Projectile projectile,
                                                         LevelTargetLimb limb,
                                                         out float damage)
        {
            damage = default;

            bool isColorsMatch = IsColorMatch(projectile, levelTarget);
            bool isRepeatedHit = hittedProjectiles.Contains(projectile);

            if (!isColorsMatch || isRepeatedHit)
            {
                return false;
            }

            hittedProjectiles.Add(projectile);
            projectile.OnShouldDestroy += Projectile_OnShouldDestroy;

            return true;
        }

        protected virtual bool IsColorMatch(Projectile projectile, LevelTarget levelTarget) =>
            ColorTypesSolutions.CanHitEnemy(projectile, levelTarget);


        private void RemoveHittedProjectile(Projectile projectile)
        {
            projectile.OnShouldDestroy -= Projectile_OnShouldDestroy;
            hittedProjectiles.Remove(projectile);
        }


        protected override void HandleCollidableObjectCollision(CollidableObject collidableObject,
                                                                LevelTargetLimb limb)
        {
            float damage;

            switch (collidableObject.Type)
            {
                case CollidableObjectType.Projectile:
                    Projectile projectile = collidableObject.Projectile;

                    if (projectile != null)
                    {
                        HandleProjectileCollision(projectile, limb, out damage);
                    }
                    else
                    {
                        CustomDebug.Log($"Wrong Collidable Object Type on {collidableObject.name}.");
                    }

                    break;

                case CollidableObjectType.PhysicalObject:
                    PhysicalLevelObject physicalLevelObject = collidableObject.PhysicalLevelObject;

                    if (physicalLevelObject != null)
                    {
                        HandlePhysicalLevelObjectCollision(physicalLevelObject, limb, out damage);
                    }
                    else
                    {
                        CustomDebug.Log($"Wrong Collidable Object Type on {collidableObject.name}.");
                    }

                    break;

                case CollidableObjectType.Monolith:
                    HandleMonolithCollision(limb, out damage);
                    break;

                case CollidableObjectType.EnemyTrigger:
                    LevelTargetLimbPart part = collidableObject.GetComponent<LevelTargetLimbPart>();
                    LevelTarget collidedLevelTarget = collidableObject.LevelTarget;

                    if (part != null &&
                        collidedLevelTarget != null &&
                        !levelTarget.Equals(collidedLevelTarget))
                    {
                        HandleAnotherLimbPartCollision(part, levelTarget, limb, out damage);
                    }
                    else
                    {
                        CustomDebug.Log($"Wrong Collidable Object Type on {collidableObject.name}.");
                        CustomDebug.Log($"Wrong Game Object Reference on {collidableObject.name}.");
                    }

                    break;

                case CollidableObjectType.AcidLiquid:
                    LiquidLevelObject liquid = collidableObject.LiquidLevelObject;

                    if (liquid != null)
                    {
                        HandleLiquidCollision(liquid, limb, out damage);
                    }
                    else
                    {
                        CustomDebug.Log($"Wrong Collidable Object Type on {collidableObject.name}.");
                    }
                    break;

                case CollidableObjectType.Spikes:
                    Spikes spikes = collidableObject.PhysicalLevelObject as Spikes;

                    if (spikes != null)
                    {
                        HandleSpikesCollision(spikes, limb, out damage);
                    }
                    else
                    {
                        CustomDebug.Log($"Wrong Collidable Object Type on {collidableObject.name}.");
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion



        #region Events handlers

        private void ProjectileReboundComponent_OnRebound(Projectile projectile)
        {
            if (hittedProjectiles.Contains(projectile))
            {
                RemoveHittedProjectile(projectile);
            }
        }


        private void Projectile_OnShouldDestroy(Projectile projectile)
        {
            RemoveHittedProjectile(projectile);
        }

        #endregion
    }
}
