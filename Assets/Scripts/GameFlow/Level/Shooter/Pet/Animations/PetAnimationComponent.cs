using System;
using System.Collections.Generic;
using Drawmasters.Effects;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Drawmasters.Levels;
using Spine;
using Spine.Unity;
using UnityEngine;
using Modules.General;


namespace Drawmasters.Pets
{
    public class PetAnimationComponent : PetComponent
    {
        #region Fields

        public static event Action OnTapReactionAnimation;

        private const int AnimationIndex = 0;

        private readonly PetAnimationSettings animationSettings;
        private readonly PetSkinsSettings skinSettings;
        private readonly LoopedInvokeTimer emotionAnimationTimer;
        private readonly IPetsService petsService;

        private ILevelEnvironment levelEnvironment;

        private EffectHandler appearEffectHandler;
        private EffectHandler disappearEffectHandler;
        private EffectHandler idleEffectHandler;
        private EffectHandler sleepEffectHandler;
        private EffectHandler wakeupEffectHandler;

        private PetLevelSettings.MoveType currentMoveType;

        private bool shouldReckonPetCharged;

        #endregion



        #region Properties

        private bool CanPlayAnimation =>
            mainPet.IsPetExists &&
            mainPet.CurrentSkinLink != null &&
            mainPet.SkeletonAnimation != null;


        private bool IsPetSleep =>
            AllowWorkWithPet &&
            !petsService.ChargeController.IsPetCharged(mainPet.Type) &&
            !shouldReckonPetCharged &&
            !petsService.TutorialController.ShouldReckonPetCharged;

        #endregion



        #region Class lifecycle

        public PetAnimationComponent()
        {
            animationSettings = IngameData.Settings.pets.animationSettings;
            skinSettings = IngameData.Settings.pets.skinsSettings;
            petsService = GameServices.Instance.PetsService;

            emotionAnimationTimer = new LoopedInvokeTimer(OnShouldPlayEmotionAnimation, animationSettings.emotionsAnimationsDelay);
        }

        #endregion



        #region Methods

        public override void Initialize(Pet mainPetValue)
        {
            base.Initialize(mainPetValue);

            levelEnvironment = GameServices.Instance.LevelEnvironment;

            bool isSceneMode = levelEnvironment.Context.SceneMode.IsSceneMode();
            bool isExcludedMode = mainPet.Shooter.CurrentGameMode.IsExcludedFromLoad();
            bool isModeOpen = levelEnvironment.Context.Mode.IsModeOpen();
            bool isBonusLevel = levelEnvironment.Context.IsBonusLevel;

            if (isSceneMode && (isExcludedMode || !isModeOpen))
            {
                return;
            }

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            mainPet.OnPreparePetSkinChange += OnPrepareSkinChanged;
            mainPet.OnPostPetSkinChange += OnSkinChanged;

            if (isSceneMode)
            {
                ForcemeterAnimationComponent.OnProgressFinishFill += ForcemeterAnimationComponent_OnProgressFinishFill;
                RouletteScreen.OnShouldRewardReceive += RouletteScreen_OnShouldRewardReceive;
                ProposalResultScreen.OnShouldRewardReceive += ProposalResultScreen_OnShouldRewardReceive;

                emotionAnimationTimer.Reset();
                emotionAnimationTimer.Start();
            }
            else
            {
                PetInvokeComponent.OnPreInvokePetForLevel += PetInvokeComponent_OnPreInvokePetForLevel;
                PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;

                PetShootComponent.OnShooted += PetShootComponent_OnShooted;

                PetMoveComponent.OnMoveTypeChanged += PetMoveComponent_OnMoveTypeChanged;
            }

            if (isBonusLevel)
            {
                PetBonusLevelComponent.OnPetSpawn += PetBonusLevelComponent_OnPetSpawn;
                PetBonusLevelComponent.OnPetBeginTeleportation += PetBonusLevelComponent_OnPetBeginTeleportation;
                PetBonusLevelComponent.OnPetEndTeleportation += PetBonusLevelComponent_OnPetEndTeleportation;
            }

            petsService.ChargeController.OnCharged += ChargeController_OnCharged;
        }


        public override void Deinitialize()
        {
            emotionAnimationTimer.Stop();

            StopFx(ref appearEffectHandler);
            StopFx(ref disappearEffectHandler);
            StopFx(ref idleEffectHandler);
            StopFx(ref wakeupEffectHandler);
            StopFx(ref sleepEffectHandler);

            UnsubscribeFromSceneEvents();
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            mainPet.OnPreparePetSkinChange -= OnPrepareSkinChanged;
            mainPet.OnPostPetSkinChange -= OnSkinChanged;

            PetInvokeComponent.OnPreInvokePetForLevel -= PetInvokeComponent_OnPreInvokePetForLevel;
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;
            petsService.ChargeController.OnCharged -= ChargeController_OnCharged;

            PetShootComponent.OnShooted -= PetShootComponent_OnShooted;

            PetMoveComponent.OnMoveTypeChanged -= PetMoveComponent_OnMoveTypeChanged;

            PetBonusLevelComponent.OnPetSpawn -= PetBonusLevelComponent_OnPetSpawn;
            PetBonusLevelComponent.OnPetBeginTeleportation -= PetBonusLevelComponent_OnPetBeginTeleportation;
            PetBonusLevelComponent.OnPetEndTeleportation -= PetBonusLevelComponent_OnPetEndTeleportation;

            if (mainPet.CurrentSkinLink != null)
            {
                mainPet.CurrentSkinLink.IngameTouchMonitor.OnUp -= IngameTouchMonitor_OnUp;
            }

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

          //  ResetFxHandlersScale();

            base.Deinitialize();
        }


        private void UnsubscribeFromSceneEvents()
        {
            ForcemeterAnimationComponent.OnProgressFinishFill -= ForcemeterAnimationComponent_OnProgressFinishFill;
            RouletteScreen.OnShouldRewardReceive -= RouletteScreen_OnShouldRewardReceive;
            ProposalResultScreen.OnShouldRewardReceive -= ProposalResultScreen_OnShouldRewardReceive;
        }


        private void PlayFx(ref EffectHandler effectHandler, string fxKey, bool isLoop, Vector3 position = default, Transform root = default)
        {
            StopFx(ref effectHandler);

            effectHandler = EffectManager.Instance.CreateSystem(fxKey, isLoop, position, default, root);

            if (effectHandler != null)
            {
                effectHandler.Play(withClear: false);
            }
        }


        private void StopFx(ref EffectHandler effectHandler)
        {
            EffectManager.Instance.ReturnHandlerToPool(effectHandler);
            effectHandler = null;
        }


        private void PlayAppearAnimation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);

            PetSkinsSettings petSkinsSettings = IngameData.Settings.pets.skinsSettings;
            mainPet.CurrentSkinLink.transform.localScale = petSkinsSettings.FindTargetLocalScaleLevel(mainPet.Type);

            SetAnimation(AnimationIndex, animationSettings.hideIdleName, false, () =>
            {
                float delay = levelEnvironment.Context.IsSceneMode ?
                    animationSettings.showOnSceneAnimationDelay : animationSettings.showOnLevelAnimationDelay;
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    if (IsPetSleep)
                    {
                        PlayStartAnimation();
                    }
                    else
                    {
                        string animationName = animationSettings.RandomAppearNames;

                        SetAnimation(AnimationIndex,
                                     animationName,
                                     false,
                                     PlayStartAnimation);
                    }

                    PlayAppearFx();
                }, delay);
            });
        }


        private void PlayDisappearAnimation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);

            SetAnimation(AnimationIndex, animationSettings.hideIdleName, false, () =>
            {
                float delay = levelEnvironment.Context.IsSceneMode ?
                    animationSettings.showOnSceneAnimationDelay : animationSettings.showOnLevelAnimationDelay;
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    SetAnimation(AnimationIndex,
                                 animationSettings.RandomDisappearNames,
                                 false,
                                 PlayStartAnimation);

                    PlayDisappearFx();
                }, delay);
            });
        }


        private void PlayStartAnimation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            if (levelEnvironment.Context.SceneMode.IsSceneMode())
            {
                PlayIdleAnimation();
            }
            else
            {
                PlayLevelStartAnimation();
            }
        }


        private void PlayLevelStartAnimation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            PlayIdleAnimation();

            TrackEntry currentTrack = mainPet.SkeletonAnimation.AnimationState.GetCurrent(AnimationIndex);
            currentTrack.AnimationEnd = animationSettings.ingameTrackEnd;

            SpineUtility.AddEndCallback(ref currentTrack, () =>
            {
                SetAnimation(AnimationIndex, animationSettings.moveAnimationKey, true);
            });
        }


        private void PlayIdleAnimation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            if (IsPetSleep)
            {
                AddAnimation(AnimationIndex, animationSettings.sleepIdleAnimationName, true);
                PlaySleepFx();
            }
            else
            {
                AddAnimation(AnimationIndex, animationSettings.idleName, true);
                PlayIdleFx();
            }
        }


        private void PlayWakeUpAnimation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            StopFx(ref sleepEffectHandler);

            SetAnimation(AnimationIndex, animationSettings.wakeUpAnimationName, false);
        }


        private void SetAnimation(int index, string animationName, bool isLoop, Action callback = null)
        {
            SetRenderSeparator(animationName);
            SpineUtility.SafeSetAnimation(mainPet.SkeletonAnimation, animationName, index, isLoop, callback);
        }


        private void AddAnimation(int index, string animationName, bool isLoop, float delay = 0, Action callback = null)
        {
            SetRenderSeparator(animationName);
            SpineUtility.SafeAddAnimation(mainPet.SkeletonAnimation, animationName, index, isLoop, delay, callback);
        }


        private void SetRenderSeparator(string animationName)
        {
            int[] renderOrders =
                IngameData.Settings.pets.renderSeparatorSettings.FindAnimation(mainPet.Type,
                    mainPet.Shooter.SkinSkeletonType, animationName);

            List<SkeletonPartsRenderer> partsRenderers = mainPet.Shooter.SkeletonRenderSeparator.partsRenderers;

            if (renderOrders == null ||
                partsRenderers == null ||
                partsRenderers.Count == 0 ||
                renderOrders.Length == 0)
            {
                mainPet.Shooter.SkeletonRenderSeparator.enabled = false;
            }
            else
            {
                mainPet.Shooter.SkeletonRenderSeparator.enabled = true;

                for (int i = 0; i < renderOrders.Length && i < partsRenderers.Count; i++)
                {
                    partsRenderers[i].MeshRenderer.sortingOrder = renderOrders[i];
                }
            }
        }


        private void ResetFxHandlerScale(EffectHandler effectHandler)
        {
            if (effectHandler != null)
            {
                effectHandler.transform.localScale = Vector3.one;
            }
        }


        private void SetFxHandlersScaleMultiplayer(EffectHandler effectHandler, float scaleMultiplayer)
        {
            if (effectHandler != null)
            {
                effectHandler.transform.localScale = Vector3.one * scaleMultiplayer;
            }
        }


        private void ResetFxHandlersScale()
        {
            ResetFxHandlerScale(appearEffectHandler);
            ResetFxHandlerScale(disappearEffectHandler);
            ResetFxHandlerScale(idleEffectHandler);
            ResetFxHandlerScale(sleepEffectHandler);
            ResetFxHandlerScale(wakeupEffectHandler);
        }

        #endregion



        #region Fx Methods

        private void PlayAppearFx()
        {
            string fxKey = skinSettings.GetAppearFxsKey(mainPet.Type, mainPet.ColorType);
            Transform fxRoot = mainPet.CurrentSkinLink.FxInOut;

            PlayFx(ref appearEffectHandler, fxKey, false, fxRoot.position, null);
        }


        private void PlayDisappearFx()
        {
            string fxKey = skinSettings.GetDisappearFxsKey(mainPet.Type, mainPet.ColorType);
            Transform fxRoot = mainPet.CurrentSkinLink.FxInOut;

            PlayFx(ref disappearEffectHandler, fxKey, false, fxRoot.position, null);
        }


        private void PlayIdleFx()
        {
            string fxKey = skinSettings.GetIdleFxsKey(mainPet.Type, mainPet.ColorType);
            Transform fxRoot = mainPet.CurrentSkinLink.FxMagic;

            PlayFx(ref idleEffectHandler, fxKey, true, fxRoot.position, fxRoot);
        }


        private void PlaySleepFx()
        {
            string fxKey = skinSettings.sleepFxKey;
            Transform fxRoot = mainPet.CurrentSkinLink.FxMagic;

            PlayFx(ref sleepEffectHandler, fxKey, true, fxRoot.position, fxRoot);
        }


        private void PlayMoveAnimation(PetLevelSettings.MoveType moveType)
        {
            string moveAnimationName = animationSettings.FindMoveAnimationName(moveType);
            SetAnimation(AnimationIndex, moveAnimationName, true);
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            if (state == LevelState.Initialized)
            {
                if (levelEnvironment.Context.SceneMode.IsSceneMode())
                {
                    PlayAppearAnimation();
                }
                else
                {
                    mainPet.CurrentSkinLink.transform.localScale = Vector3.zero;
                }
            }
            else if (state == LevelState.Finished)
            {
                UnsubscribeFromSceneEvents();
            }
            else if (state == LevelState.AllTargetsHitted)
            {
                PetMoveComponent.OnMoveTypeChanged -= PetMoveComponent_OnMoveTypeChanged;

                string winAnimationName = animationSettings.RandomIngameWinAnimationName;
                SetAnimation(AnimationIndex, winAnimationName, false, () => PlayMoveAnimation(PetLevelSettings.MoveType.Slow));
            }
        }


        private void ForcemeterAnimationComponent_OnProgressFinishFill(int iterationIndex)
        {
            if (!CanPlayAnimation || IsPetSleep)
            {
                return;
            }

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);
            AddAnimation(AnimationIndex, animationSettings.RandomForcemeterNames, false, 0.0f, PlayIdleAnimation);
        }


        private void ProposalResultScreen_OnShouldRewardReceive(RewardData rewardData)
        {
            if (!CanPlayAnimation || IsPetSleep)
            {
                return;
            }

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);
            AddAnimation(AnimationIndex, animationSettings.RandomPremiumStoreNames, false, 0.0f, PlayIdleAnimation);
        }


        private void RouletteScreen_OnShouldRewardReceive(RewardData rewardData)
        {
            if (!CanPlayAnimation || IsPetSleep)
            {
                return;
            }

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);
            AddAnimation(AnimationIndex, animationSettings.RandomStoreNames, false, 0.0f, PlayIdleAnimation);
        }


        private void IngameTouchMonitor_OnUp()
        {
            bool canPlaySleepStartAnimation = animationSettings.CanPlaySleepStartAnimation(mainPet.Type);

            if (IsPetSleep && !canPlaySleepStartAnimation)
            {
                return;
            }

            mainPet.CurrentSkinLink.IngameTouchMonitor.OnUp -= IngameTouchMonitor_OnUp;

            emotionAnimationTimer.Reset();
            emotionAnimationTimer.Stop();

            string tapReactiongAnimationName = IsPetSleep ?
                animationSettings.RandomTapReactionSleepAnimationNames(mainPet.Type) : animationSettings.RandomTapReactionAnimationName(mainPet.Type);

            SetAnimation(AnimationIndex, tapReactiongAnimationName, false, () =>
            {
                emotionAnimationTimer.Start();

                if (IsPetSleep)
                {
                    AddAnimation(AnimationIndex, animationSettings.sleepStartAnimationName, false, 0.0f, PlayIdleAnimation);
                }
                else
                {
                    PlayIdleAnimation();
                }

                mainPet.CurrentSkinLink.IngameTouchMonitor.OnUp += IngameTouchMonitor_OnUp;
            });

            OnTapReactionAnimation?.Invoke();
        }


        private void PetChargeButton_OnEnableButton()
        {
            if (!IsPetSleep)
            {
                return;
            }

            mainPet.CurrentSkinLink.PetChargeButton.OnEnableButton -= PetChargeButton_OnEnableButton;
            mainPet.CurrentSkinLink.PetChargeButton.OnDisableButton += PetChargeButton_OnDisableButton;

            StopFx(ref sleepEffectHandler);
        }


        private void PetChargeButton_OnDisableButton()
        {
            if (!IsPetSleep)
            {
                return;
            }

            mainPet.CurrentSkinLink.PetChargeButton.OnEnableButton += PetChargeButton_OnEnableButton;
            mainPet.CurrentSkinLink.PetChargeButton.OnDisableButton -= PetChargeButton_OnDisableButton;

            PlaySleepFx();
        }


        private void ChargeController_OnCharged(PetSkinType petSkinType)
        {
            if (CanPlayAnimation && 
                mainPet.Type == petSkinType)
            {
                PlayWakeUpAnimation();
            }
        }


        private void OnSkinChanged()
        {
            if (levelEnvironment.Context.IsSceneMode &&
                CanPlayAnimation)
            {
                mainPet.CurrentSkinLink.IngameTouchMonitor.OnUp += IngameTouchMonitor_OnUp;
                mainPet.CurrentSkinLink.PetChargeButton.OnEnableButton += PetChargeButton_OnEnableButton;
                mainPet.CurrentSkinLink.PetChargeButton.OnDisableButton += PetChargeButton_OnDisableButton;
            }

            StopFx(ref appearEffectHandler);
            StopFx(ref idleEffectHandler);

            if (levelEnvironment.Context.IsSceneMode)
            {
                PlayAppearAnimation();
            }
        }


        private void OnPrepareSkinChanged()
        {
            if (mainPet.CurrentSkinLink != null)
            {
                mainPet.CurrentSkinLink.IngameTouchMonitor.OnUp -= IngameTouchMonitor_OnUp;
                mainPet.CurrentSkinLink.PetChargeButton.OnEnableButton -= PetChargeButton_OnEnableButton;
                mainPet.CurrentSkinLink.PetChargeButton.OnDisableButton -= PetChargeButton_OnDisableButton;
            }

            mainPet.PetSkinChange(GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin);
        }


        private void PetInvokeComponent_OnPreInvokePetForLevel(Pet anotherPet)
        {
            if (mainPet != anotherPet ||
                !CanPlayAnimation)
            {
                return;
            }

            mainPet.CurrentSkinLink.transform.localScale = Vector3.zero;
            SetAnimation(AnimationIndex, animationSettings.idleName, true);
        }


        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet anotherPet)
        {
            if (mainPet != anotherPet || !CanPlayAnimation)
            {
                return;
            }

            shouldReckonPetCharged = true;
            PlayAppearAnimation();
        }


        private void PetShootComponent_OnShooted(Pet anotherPet, Vector3 targetPosition)
        {
            if (mainPet != anotherPet || !CanPlayAnimation)
            {
                return;
            }

            SetAnimation(AnimationIndex, animationSettings.RandomShootAnimationName, false, () => PlayMoveAnimation(currentMoveType));
        }


        private void PetMoveComponent_OnMoveTypeChanged(Pet anotherPet, PetLevelSettings.MoveType moveType)
        {
            if (mainPet != anotherPet || !CanPlayAnimation)
            {
                return;
            }

            currentMoveType = moveType;
            PlayMoveAnimation(currentMoveType);
        }


        private void OnShouldPlayEmotionAnimation()
        {
            if (!CanPlayAnimation || IsPetSleep)
            {
                return;
            }

            emotionAnimationTimer.Stop();
            SetAnimation(AnimationIndex, animationSettings.RandomEmotionsAnimationName, false, () =>
            {
                emotionAnimationTimer.Start();
                PlayIdleAnimation();
            });
        }


        private void PetBonusLevelComponent_OnPetSpawn()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);

            PetSkinsSettings petSkinsSettings = IngameData.Settings.pets.skinsSettings;
            mainPet.CurrentSkinLink.transform.localScale = petSkinsSettings.FindTargetLocalScaleBonusLevelAppear(mainPet.Type);

            PlayAppearFx();

            float commonScaleMagnitude = petSkinsSettings.FindTargetLocalScaleLevel(mainPet.Type).magnitude;
            float currentScaleMagnitude = mainPet.CurrentSkinLink.transform.localScale.magnitude;

            float scaleMultiplayer = Mathf.Approximately(commonScaleMagnitude, 0.0f) ? default : currentScaleMagnitude / commonScaleMagnitude;

            SetFxHandlersScaleMultiplayer(appearEffectHandler, scaleMultiplayer);

            SetAnimation(AnimationIndex, animationSettings.RandomIngameWinAnimationName, false);
        }


        private void PetBonusLevelComponent_OnPetBeginTeleportation() =>
            PlayDisappearAnimation();


        private void PetBonusLevelComponent_OnPetEndTeleportation()
        {
            if (!CanPlayAnimation)
            {
                return;
            }

            ResetFxHandlerScale(appearEffectHandler);
            ResetFxHandlerScale(disappearEffectHandler);

            SpineUtility.SafeRefreshSkeleton(mainPet.SkeletonAnimation);

            PetSkinsSettings petSkinsSettings = IngameData.Settings.pets.skinsSettings;
            mainPet.CurrentSkinLink.transform.localScale = petSkinsSettings.FindTargetLocalScaleLevel(mainPet.Type);

            PlayAppearFx();
            SetAnimation(AnimationIndex, animationSettings.idleName, true);
        }

        #endregion
    }
}
