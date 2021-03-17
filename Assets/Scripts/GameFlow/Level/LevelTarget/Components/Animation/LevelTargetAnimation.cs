using System;
using Spine;
using Spine.Unity;
using Animation = Spine.Animation;
using Random = UnityEngine.Random;


namespace Drawmasters.Levels
{
    public abstract class LevelTargetAnimation : LevelTargetComponent
    {
        #region Fields

        protected const int MainIndex = 0;
        protected const int FaceIndex = 1;
        protected const int ColorIndex = 3;

        protected readonly EnemyAnimationNames animationNames;
        protected readonly SkeletonAnimation skeletonAnimation;

        private readonly int minIdlesCount = 0;
        private readonly int maxIdlesCount = 0;

        int idlesCountForSpecialIdle;
        int idlesCount;

        string specialIdleName;

        EnemyAnimationNames.ComplexAnimation painAnimation;
        EnemyAnimationNames.ComplexAnimation panicAnimation;

        bool isCharacterAimingAtEnemy;

        #endregion



        #region Properties

        private int GetRandomIdlesCount => Random.Range(minIdlesCount, maxIdlesCount + 1);

        protected virtual string CharacterLoseAnimation => animationNames.RandomDanceAnimation;

        #endregion



        #region Lifecycle

        public LevelTargetAnimation(SkeletonAnimation _skeletonAnimation,
                                    EnemyAnimationNames _enemyAnimationNames)
        {
            skeletonAnimation = _skeletonAnimation;
            animationNames = _enemyAnimationNames;

            minIdlesCount = IngameData.Settings.levelTarget.minIdlesCountBeforeSpecialIdle;
            maxIdlesCount = IngameData.Settings.levelTarget.maxIdlesCountBeforeSpecialIdle;
        }

        #endregion



        #region Methods

        public override void Initialize(LevelTarget _levelTarget)
        {
            base.Initialize(_levelTarget);

            idlesCountForSpecialIdle = GetRandomIdlesCount;
            specialIdleName = string.Empty;
            panicAnimation = null;
        }


        public override void Enable()
        {
            levelTarget.OnShouldApplyRagdoll += LevelTarget_OnShouldApplyRagdoll;
            skeletonAnimation.AnimationState.Complete += AnimationState_Complete;

            ClearAnimationState();

            PlayAnimation(animationNames.IdleAnimationName, true, MainIndex);

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        public override void Disable()
        {
            DisableOnAimAnimations();

            skeletonAnimation.AnimationState.Complete -= AnimationState_Complete;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        protected void PlayAnimation(string name, bool isLoop, int index, Action callback = null)
        {
            if (skeletonAnimation == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (index == FaceIndex || index == ColorIndex)
            {
                skeletonAnimation.AnimationState.ClearTrack(index);
            }
            else
            {
                if (levelTarget.Ragdoll2D.IsActive)
                {
                    return;
                }
            }

            Animation animation = skeletonAnimation.Skeleton.Data.FindAnimation(name);
            if (animation == null)
            {
                CustomDebug.Log("Cannot find animation. Name " + name);
                return;
            }

            TrackEntry tr = skeletonAnimation.AnimationState.SetAnimation(index, animation, isLoop);


        }


        protected void DisableOnAimAnimations()
        {
            if (!levelTarget.IsHitted &&
                !levelTarget.Ragdoll2D.IsActive)
            {
                ShooterEnemyAiming.OnAimAtEnemy -= ShooterEnemyAiming_OnAimAtEnemy;
            }
        }


        protected void AddAnimation(string name, bool isLoop, int index) =>
            AddAnimation(name, isLoop, index, 0f);
        

        protected void AddAnimation(string name, bool isLoop, int index, float delay)
        {
            if (skeletonAnimation == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            Animation animation = skeletonAnimation.Skeleton.Data.FindAnimation(name);
            if (animation == null)
            {
                return;
            }

            skeletonAnimation.AnimationState.AddAnimation(index,
                                                          animation,
                                                          isLoop,
                                                          delay);
        }


        protected void EnablePanicOnAim() =>
                ShooterEnemyAiming.OnAimAtEnemy += ShooterEnemyAiming_OnAimAtEnemy;


        protected void ClearAnimationState()
        {
            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.AnimationState.Update(default);
            skeletonAnimation.Skeleton.SetToSetupPose();
        }

        #endregion


        #region Event handlers


        private void ShooterEnemyAiming_OnAimAtEnemy(LevelTarget enemy)
        {
            isCharacterAimingAtEnemy = (enemy != null && enemy.Equals(levelTarget));

            if (panicAnimation != null)
            {
                return;
            }

            if (isCharacterAimingAtEnemy)
            {
                panicAnimation = animationNames.RandomPanicAnimation;

                PlayAnimation(panicAnimation.start, false, MainIndex);
                AddAnimation(panicAnimation.main, true, MainIndex);
            }
        }


        private void LevelTarget_OnShouldApplyRagdoll(LevelTarget otherLevelTarget)
        {
            skeletonAnimation.AnimationState.ClearTracks();

            painAnimation = animationNames.RandomFacePainAnimation;

            PlayAnimation(painAnimation.start, false, FaceIndex);
            AddAnimation(painAnimation.main, true, FaceIndex);

            levelTarget.OnShouldApplyRagdoll -= LevelTarget_OnShouldApplyRagdoll;
            ShooterEnemyAiming.OnAimAtEnemy -= ShooterEnemyAiming_OnAimAtEnemy;
        }

        #endregion



        #region Animation event handler

        private void AnimationState_Complete(TrackEntry trackEntry)
        {
            string completeAnimationName = trackEntry.Animation.Name;

            if (completeAnimationName == animationNames.IdleAnimationName)
            {
                CheckIdleAnimation();
            }
            else if (completeAnimationName == specialIdleName)
            {
                CheckSpecialIdleAnimation();
            }
            else if (panicAnimation != null &&
                     completeAnimationName == panicAnimation.main &&
                     !isCharacterAimingAtEnemy)
            {
                PlayAnimation(panicAnimation.end, false, MainIndex);
                AddAnimation(animationNames.IdleAnimationName, true, MainIndex);
            }
            else if (panicAnimation != null &&
                     completeAnimationName == panicAnimation.end)
            {
                panicAnimation = null;
            }
        }


        private void CheckIdleAnimation()
        {
            idlesCount++;

            if (idlesCount >= idlesCountForSpecialIdle)
            {
                specialIdleName = animationNames.RandomSpecialIdleAnimation;

                PlayAnimation(specialIdleName, false, MainIndex);

                idlesCount = 0;
                idlesCountForSpecialIdle = GetRandomIdlesCount;
            }
        }


        private void CheckSpecialIdleAnimation()
        {
            specialIdleName = string.Empty;

            PlayAnimation(animationNames.IdleAnimationName, true, MainIndex);
        }

        #endregion



        #region Events handlers

        protected virtual void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.FriendlyDeath ||
                levelState == LevelState.OutOfAmmo)
            {
                PlayAnimation(CharacterLoseAnimation, true, MainIndex);
            }
        }


        protected void PlayBurnAnimation()
        {
            levelTarget.OnShouldStartLaserDestroy -= PlayBurnAnimation;

            DisableOnAimAnimations();

            skeletonAnimation.AnimationState.ClearTracks();

            var faceDeathAnimation = animationNames.RandomFaceBurnAnimation;

            PlayAnimation(faceDeathAnimation.start, false, FaceIndex);
            AddAnimation(faceDeathAnimation.main, true, FaceIndex);

            PlayAnimation(animationNames.BurnAnimationName, false, ColorIndex);

            skeletonAnimation.Update(0.0f);

        }

        #endregion
    }
}
