using Modules.Sound;
using Spine;
using System;
using Drawmasters.ServiceUtil;
using Drawmasters.Levels.Data;
using Modules.General;
using Drawmasters.Utils;
using Drawmasters.Pets;

namespace Drawmasters.Levels
{
    public class ShooterAnimation : ShooterComponent
    {
        #region Fields

        public static event Action OnShouldEnableAttachments;

        public const int AnimationIndex = 0;

        public const string IdleEmptyAnimation = "idle_empty";

        private readonly LoopedInvokeTimer emotionAnimationTimer;

        private readonly string greetingSound;
        private readonly float greetingSoundDelay;
        private readonly ShooterAnimationSettings animationSettings;

        private TrackEntry greetingTracker;
        private WeaponSkinType currentWeaponSkinType;
        private WeaponAnimationType currentWeaponAnimationType;

        private bool isMainMenu;

        #endregion


        #region Lifecycle

        public ShooterAnimation(string _greetingSound,
                                float _greetingSoundDelay)
        {
            greetingSound = _greetingSound;
            greetingSoundDelay = _greetingSoundDelay;

            animationSettings = IngameData.Settings.shooterAnimationSettings;
            emotionAnimationTimer = new LoopedInvokeTimer(OnShouldPlayEmotionAnimation, animationSettings.emotionsAnimationsDelay);
        }

        #endregion



        #region Methods

        public override void StartGame()
        {
            LevelContext levelContext = GameServices.Instance.LevelEnvironment.Context;

            bool isSceneMode = levelContext.SceneMode.IsSceneMode();
            isMainMenu = levelContext.SceneMode == GameMode.MenuScene;
            bool isExcludedMode = shooter.CurrentGameMode.IsExcludedFromLoad();
            bool isModeOpen = levelContext.Mode.IsModeOpen();

            if (isMainMenu)
            {
                emotionAnimationTimer.Reset();
                emotionAnimationTimer.Start();

                StartMonitorTouchAnimation();
            }

            if (isSceneMode && (isExcludedMode || !isModeOpen))
            {
                return;
            }

            ShooterSkinComponent.OnWeaponSkinApplied += ShooterSkinComponent_OnWeaponSkinApplied;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel += PetsInvokeController_OnInvokePetForLevel;
        }


        public override void Deinitialize()
        {
            GameServices.Instance.LevelControllerService.Target.OnTargetHitted -= TargetController_OnTargetHitted;

            ShooterSkinComponent.OnWeaponSkinApplied -= ShooterSkinComponent_OnWeaponSkinApplied;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel -= PetsInvokeController_OnInvokePetForLevel;

            StopMonitorTouchAnimation();

            if (greetingTracker != null)
            {
                greetingTracker.Event -= TrackEntry_Event;
            }

            emotionAnimationTimer.Stop();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        private void StartMonitorTouchAnimation()
        {
            PetAnimationComponent.OnTapReactionAnimation += PetAnimationComponent_OnTapReactionAnimation;
            shooter.IngameTouchMonitor.OnUp += IngameTouchMonitor_OnUp;
        }


        private void StopMonitorTouchAnimation()
        {
            PetAnimationComponent.OnTapReactionAnimation -= PetAnimationComponent_OnTapReactionAnimation;
            shooter.IngameTouchMonitor.OnUp -= IngameTouchMonitor_OnUp;
        }


        private void PlayGreetingAnimation(WeaponSkinType weaponSkinType)
        {
            WeaponAnimationType weaponAnimationType = IngameData.Settings.weaponSkinSettings.GetAnimationType(weaponSkinType);
            string animationName = IngameData.Settings.shooterAimingSettings.GetRandomGreetingAnimation(weaponAnimationType);

            if (greetingTracker != null)
            {
                greetingTracker.Event -= TrackEntry_Event;
            }

            greetingTracker = SetAnimation(AnimationIndex, animationName, false);
            AddIdleAnimation();

            if (greetingTracker != null)
            {
                greetingTracker.Event += TrackEntry_Event;
            }

            Scheduler.Instance.CallMethodWithDelay(this, () => SoundManager.Instance.PlaySound(greetingSound), greetingSoundDelay);
        }


        private void PlayWeaponHideAnimation()
        {
            string animationName = animationSettings.FindHideWeaponAnimation(currentWeaponAnimationType);
            SetAnimation(AnimationIndex, animationName, false, AddIdleWithoutWeaponAnimation);
        }


        private void PlayFearAnimation(WeaponSkinType weaponSkinType)
        {
            WeaponAnimationType weaponAnimationType = IngameData.Settings.weaponSkinSettings.GetAnimationType(weaponSkinType);
            string animationName = IngameData.Settings.shooterAnimationSettings.RandomFearAnimation;

            TrackEntry trakcer = SetAnimation(AnimationIndex, animationName, false);

            trakcer.Complete += FearTrakcer_Complete;
        }

        private void FearTrakcer_Complete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= FearTrakcer_Complete;

            SpineUtility.SafeRefreshSkeleton(shooter.SkeletonAnimation);
            PlayGreetingAnimation(currentWeaponSkinType);
        }

        private void PlayWinAnimation()
        {
#warning hot fix
            if (shooter == null)
            {
                CustomDebug.LogError($"shooter is NULL. Bug detected");
                return;
            }

            SpineUtility.SafeRefreshSkeleton(shooter.SkeletonAnimation);
            SetAnimation(AnimationIndex, animationSettings.RandomWinName, false);
        }


        private void PlayLoseAnimation()
        {
#warning hot fix
            if (shooter == null)
            {
                CustomDebug.LogError($"shooter is NULL. Bug detected");
                return;
            }

            SpineUtility.SafeRefreshSkeleton(shooter.SkeletonAnimation);
            SetAnimation(AnimationIndex, animationSettings.RandomLoseName, false);
        }


        private TrackEntry SetAnimation(int index, string name, bool isLoop, Action callback = default)
        {
#warning hot fix
            if (shooter == null)
            {
                CustomDebug.LogError($"shooter is NULL. Bug detected");
                return default;
            }

            TrackEntry trackEntry = SpineUtility.SafeSetAnimation(shooter.SkeletonAnimation, name, index, isLoop, callback);
            return trackEntry;
        }


        private void AddIdleAnimation()
        {
            string idleAnimationName = IngameData.Settings.shooterAimingSettings.GetIdleAnimation(currentWeaponAnimationType);
            shooter.SkeletonAnimation.AnimationState.AddAnimation(AnimationIndex, idleAnimationName, true, 0);
        }


        private void AddIdleWithoutWeaponAnimation()
        {
            string animationName = IngameData.Settings.shooterAimingSettings.GetAnimationWithoutWeapon();
            shooter.SkeletonAnimation.AnimationState.AddAnimation(AnimationIndex, animationName, true, 0);
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.StageChanging:
                case LevelState.AllTargetsHitted:
                    PlayWinAnimation();
                    break;

                case LevelState.OutOfAmmo:
                case LevelState.FriendlyDeath:
                    PlayLoseAnimation();
                    break;

                default:
                    break;
            }
        }


        private void PetAnimationComponent_OnTapReactionAnimation()
        {
            StopMonitorTouchAnimation();

            emotionAnimationTimer.Reset();
            emotionAnimationTimer.Stop();

            string animationName = animationSettings.FindPetTapReactionAnimation(currentWeaponAnimationType);
            SetAnimation(AnimationIndex, animationName, false, () =>
            {
                emotionAnimationTimer.Start();
                AddIdleAnimation();

                StartMonitorTouchAnimation();
            });
        }


        private void ShooterSkinComponent_OnWeaponSkinApplied(WeaponSkinType weaponSkinType)
        {
            currentWeaponSkinType = weaponSkinType;
            currentWeaponAnimationType = IngameData.Settings.weaponSkinSettings.GetAnimationType(currentWeaponSkinType);

            if (GameServices.Instance.LevelEnvironment.Context.IsBossLevel)
            {
                PlayFearAnimation(weaponSkinType);
            }
            else
            {
                if (isMainMenu)
                {
                    StopMonitorTouchAnimation();
                    StartMonitorTouchAnimation();
                }

                PlayGreetingAnimation(weaponSkinType);
            }
        }


        private void TrackEntry_Event(TrackEntry trackEntry, Event e)
        {
            string enableAttachEvent = IngameData.Settings.shooterAimingSettings.attachmentsEnableEvent;

            if (e.ToString().Equals(enableAttachEvent))
            {
                OnShouldEnableAttachments?.Invoke();
            }
        }


        private void IngameTouchMonitor_OnUp()
        {
            StopMonitorTouchAnimation();

            emotionAnimationTimer.Reset();
            emotionAnimationTimer.Stop();

            string animationName = animationSettings.FindTapReactionAnimation(currentWeaponAnimationType);

            SetAnimation(AnimationIndex, animationName, false, () =>
            {
                emotionAnimationTimer.Start();

                AddIdleAnimation();

                StartMonitorTouchAnimation();
            });
        }



        private void OnShouldPlayEmotionAnimation()
        {
            StopMonitorTouchAnimation();

            emotionAnimationTimer.Stop();

            SetAnimation(AnimationIndex, animationSettings.RandomDanceAnimation, false, () =>
            {
                emotionAnimationTimer.Start();

                PlayGreetingAnimation(currentWeaponSkinType);

                StartMonitorTouchAnimation();
            });
        }


        private void PetsInvokeController_OnInvokePetForLevel(PetSkinType petSkinType)
        {
            PlayWeaponHideAnimation();

            GameServices.Instance.LevelControllerService.Target.OnTargetHitted += TargetController_OnTargetHitted;
        }


        private void TargetController_OnTargetHitted(LevelTargetType type)
        {
            bool isAllEnemiesHitted = GameServices.Instance.LevelControllerService.Target.IsAllEnemiesHitted();
            if (type.IsEnemy() && !isAllEnemiesHitted)
            {
                SetAnimation(AnimationIndex, animationSettings.reactionPetEnemyHit, false, AddIdleWithoutWeaponAnimation);
            }
        }

        #endregion
    }
}
