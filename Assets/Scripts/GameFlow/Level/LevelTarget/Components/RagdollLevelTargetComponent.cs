using System;
using Spine;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class RagdollLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<LevelTarget> OnRagdollApplied;
           
        #endregion



        #region Overrided methods

        public override void Initialize(LevelTarget _levelTarget)
        {
            base.Initialize(_levelTarget);

            ApplyRagdollSettings();
        }


        public override void Enable()
        {
            levelTarget.OnShouldApplyRagdoll += ApplyRagdoll;
        }


        public override void Disable()
        {
            levelTarget.OnShouldApplyRagdoll -= ApplyRagdoll;
        }

        #endregion



        #region Methods

        private void ApplyRagdoll(LevelTarget otherLevelTarget)
        {
            otherLevelTarget.Ragdoll2D.RefreshTargetSkeleton();

            if (!otherLevelTarget.Ragdoll2D.IsActive)
            {
                Vector2 standVelocity = otherLevelTarget.StandPreviousFrameRigidbody2D.Velocity;

                Skin currentSkin = otherLevelTarget.SkeletonAnimation.Skeleton.Skin;
                string skinWithBoundingBoxes = IngameData.Settings.levelTargetSkinsSettings.skinWithBoundingBoxes;
                otherLevelTarget.SkeletonAnimation.Skeleton.SetSkin(skinWithBoundingBoxes);
                otherLevelTarget.Ragdoll2D.Apply();
                otherLevelTarget.SkeletonAnimation.Skeleton.SetSkin(currentSkin);

                PhysicsMaterial2D ragdollMaterial = IngameData.Settings.levelTarget.ragdollMaterial;

                Rigidbody2D[] allRigidbodies = otherLevelTarget.Ragdoll2D.RigidbodyArray;

                foreach (var ragdollRigidbody in allRigidbodies)
                {
                    ragdollRigidbody.sharedMaterial = ragdollMaterial;
                    ragdollRigidbody.velocity = standVelocity;
                    ragdollRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                }

                OnRagdollApplied?.Invoke(otherLevelTarget);
            }
        }


        private void ApplyRagdollSettings()
        {
            levelTarget.Ragdoll2D.rootMass = IngameData.Settings.levelTarget.ragdollRootMass;
            levelTarget.Ragdoll2D.rotationLimit = IngameData.Settings.levelTarget.ragdollRotationLimit;
            levelTarget.Ragdoll2D.gravityScale = IngameData.Settings.levelTarget.enemyGravityScale;
        }

        #endregion
    }
}
