using Spine.Unity;
using Spine;


namespace Drawmasters.Levels
{
    public class EnemyAnimation : LevelTargetAnimation
    {
        public EnemyAnimation(SkeletonAnimation _skeletonAnimation, EnemyAnimationNames _enemyAnimationNames) :
            base(_skeletonAnimation, _enemyAnimationNames)
        {
        }


        public override void Enable()
        {
            base.Enable();

            levelTarget.OnHitted += LevelTarget_OnHitted;
            levelTarget.OnShouldStartLaserDestroy += OnShouldPlayBurnAnimation;
            ProjectileSmashApplyComponent.OnSmashProjectile += ProjectileSmashApplyComponent_OnSmashProjectile;
            ProjectileStayComponent.OnProjectileStay += ProjectileStayComponent_OnProjectileStay;

            EnablePanicOnAim();
        }


        public override void Disable()
        {
            if (!levelTarget.IsHitted)
            {
                levelTarget.OnHitted -= LevelTarget_OnHitted;
                levelTarget.OnShouldStartLaserDestroy -= OnShouldPlayBurnAnimation;
                ProjectileSmashApplyComponent.OnSmashProjectile -= ProjectileSmashApplyComponent_OnSmashProjectile;
                ProjectileStayComponent.OnProjectileStay -= ProjectileStayComponent_OnProjectileStay;
            }

            base.Disable();
        }


        private void PlayLaughtAnimation()
        {
            ProjectileSmashApplyComponent.OnSmashProjectile -= ProjectileSmashApplyComponent_OnSmashProjectile;
            ProjectileStayComponent.OnProjectileStay -= ProjectileStayComponent_OnProjectileStay;

            PlayAnimation(animationNames.RandomLaughtAnimation, true, MainIndex);
        }


        private void LevelTarget_OnHitted(LevelTarget levelTarget)
        {
            skeletonAnimation.AnimationState.ClearTracks();

            SetDeathSlotsToSetupPose();

            string faceDeathAnimation = animationNames.RandomFaceDeathAnimation;
            PlayAnimation(faceDeathAnimation, true, FaceIndex);

            skeletonAnimation.Update(0.0f);

            skeletonAnimation.AnimationState.ClearTracks();

            levelTarget.OnHitted -= LevelTarget_OnHitted;
            DisableOnAimAnimations();
        }


        private void SetDeathSlotsToSetupPose()
        {
            foreach (var s in animationNames.slotsToRefreshOnDeath)
            {
                Slot slot = skeletonAnimation.Skeleton.FindSlot(s);

                if (slot != null)
                {
                    slot.SetToSetupPose();
                }
            }
        }


        private void OnShouldPlayBurnAnimation()
        {
            levelTarget.OnHitted -= LevelTarget_OnHitted;
            levelTarget.OnShouldStartLaserDestroy -= OnShouldPlayBurnAnimation;

            PlayBurnAnimation();
        }


        private void ProjectileSmashApplyComponent_OnSmashProjectile(Projectile smashedProjectile) =>
            PlayLaughtAnimation();


        private void ProjectileStayComponent_OnProjectileStay(Projectile stayed) =>
            PlayLaughtAnimation();
    }
}
