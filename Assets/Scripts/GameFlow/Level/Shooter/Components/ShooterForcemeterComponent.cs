using Drawmasters.Effects;
using Modules.Sound;
using Drawmasters.Ui;
using Spine;
using System;
using UnityEngine;
using Animation = Spine.Animation;
using TransformMode = Drawmasters.Effects.TransformMode;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Levels
{
    public class ShooterForcemeterComponent : ShooterComponent
    {
        #region Fields

        public static event Action<Transform> OnShouldEnableLightning; // hammer root

        private const int AnimationIndex = 0;

        private Action animationCallback;
        private TrackEntry animationTracker;

        private EffectHandler effectHandler;

        private ILevelEnvironment levelEnvironment;

        #endregion



        #region Methods

        public override void StartGame()
        {
            levelEnvironment = GameServices.Instance.LevelEnvironment;

            bool isForcemeterScene = levelEnvironment.Context.SceneMode == GameMode.ForcemeterScene;

            if (isForcemeterScene)
            {
                Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            }
        }

        public override void Deinitialize()
        {
            StopEffect();
            StopCheckEvents(animationTracker);

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            ForceMeterScreen.OnShouldPlayHitAnimation -= ForceMeterScreen_OnShouldPlayHitAnimation;
        }


        private void PlayIdleAnimation()
        {
            string animationName = IngameData.Settings.shooterAnimationSettings.forcemeterIdleName;
            PlayAnimation(animationName, true);
        }


        private TrackEntry PlayAnimation(string animationName, bool isLoop, Action callback = null)
        {
            animationCallback = null;
            Animation animation = shooter.SkeletonAnimation.Skeleton.Data.FindAnimation(animationName);

            if (animation == null)
            {
                CustomDebug.Log($"Not found anim data. Name = {animationName}");
            }

            StopCheckEvents(animationTracker);
            animationTracker = shooter.SkeletonAnimation.AnimationState.SetAnimation(AnimationIndex, animationName, isLoop);

            animationCallback = callback;
            animationTracker.Complete += AnimationTracker_Complete;

            return animationTracker;
        }


        private void AnimationTracker_Complete(TrackEntry trackEntry)
        {
            animationTracker.Complete -= AnimationTracker_Complete;

            animationCallback?.Invoke();

            animationCallback = null;
        }


        private void StopEffect()
        {
            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }
        }

        private void StartCheckEvents(TrackEntry tracker)
        {
            if (tracker != null)
            {
                tracker.Event += AnimationTracker_Event;
            }
        }


        private void StopCheckEvents(TrackEntry tracker)
        {
            if (tracker != null)
            {
                tracker.Event -= AnimationTracker_Event;
            }
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                shooter.SkeletonAnimation.ClearState();

                PlayIdleAnimation();

                ForceMeterScreen.OnShouldPlayHitAnimation += ForceMeterScreen_OnShouldPlayHitAnimation;
            }
        }


        private void ForceMeterScreen_OnShouldPlayHitAnimation(int iterationIndex)
        {
            shooter.SkeletonAnimation.ClearState();

            string animationName = IngameData.Settings.shooterAnimationSettings.GetForcemeterAnimation(iterationIndex);

            PlayAnimation(animationName, false, () =>
            {
                StopEffect();

                string effectKey = IngameData.Settings.forceMeterUiSettings.FindShooterEffectKey(iterationIndex);
                effectHandler = EffectManager.Instance.CreateSystem(effectKey, true, parent: shooter.ForcemeterFxRoot,
                                                                    transformMode: TransformMode.Local, shouldOverrideLoops: false);

                if (effectHandler != null)
                {
                    effectHandler.Play();
                }

                string sfxKey = IngameData.Settings.forceMeterUiSettings.FindShooterSfxKey(iterationIndex);
                SoundManager.Instance.PlayOneShot(sfxKey);

                PlayIdleAnimation();
            });

            StartCheckEvents(animationTracker);
        }


        private void AnimationTracker_Event(TrackEntry trackEntry, Spine.Event e)
        {
            if (e.ToString().Equals(IngameData.Settings.shooterAnimationSettings.hammerLightingEvent))
            {
                OnShouldEnableLightning?.Invoke(shooter.ForcemeterHammerFxRoot);
            }
        }

        #endregion
    }
}
