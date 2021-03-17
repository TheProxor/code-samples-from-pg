using UnityEngine;
using System.Collections.Generic;
using Modules.General;


namespace Drawmasters.Levels
{
    public abstract class LimbsCollisionHandler : LevelTargetComponent
    {
        #region Fields

        protected readonly List<LevelTargetLimb> enemyLimbs = default;

        #endregion



        #region Lifecycle

        protected LimbsCollisionHandler(List<LevelTargetLimb> _enemyLimbs)
        {
            enemyLimbs = _enemyLimbs;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            EnableCollision();
            ProjectilePullThrowComponent.OnObjectPull += ProjectilePullThrowComponent_OnObjectPull;
        }


        public override void Disable()
        {
            DisableCollision();
            ProjectilePullThrowComponent.OnObjectPull -= ProjectilePullThrowComponent_OnObjectPull;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        protected abstract void HandleCollidableObjectCollision(CollidableObject collidableObject, 
                                                                LevelTargetLimb limb);
        
        protected string ParentBoneName (string childBoneName)
        {
            string result = string.Empty;

            foreach (var limb in enemyLimbs)
            {
                bool isMatch = limb.ContainLimbPart(childBoneName);
                if (isMatch)
                {
                    result = limb.RootBoneName;
                    break;
                }
            }
            return result;
        }


        private void EnableCollision()
        {
            foreach (var limb in enemyLimbs)
            {
                limb.OnCollidableObjectHitted += Limb_OnCollidableObjectHitted;
            }
        }


        private void DisableCollision()
        {
            foreach (var limb in enemyLimbs)
            {
                limb.OnCollidableObjectHitted -= Limb_OnCollidableObjectHitted;
            }
        }

        #endregion



        #region Events handlers

        private void Limb_OnCollidableObjectHitted(CollidableObject collidableObject, 
                                                   LevelTargetLimb limb)
        {
            HandleCollidableObjectCollision(collidableObject, limb);
        }


        private void ProjectilePullThrowComponent_OnObjectPull(LevelObject pulledLevelTarget, Rigidbody2D rigidbody2D)
        {
            if (levelTarget == pulledLevelTarget)
            {
                var settings = IngameData.Settings.modesInfo.GetSettings(WeaponType.HitmastersGravitygun) as HitmastersGravitygunSettings;
                float disableDuration = (settings == null) ? default : settings.disableCollisionDurationAfterPull;

                DisableCollision();
                Scheduler.Instance.CallMethodWithDelay(this, EnableCollision, disableDuration);
            }
        }

        #endregion
    }
}
