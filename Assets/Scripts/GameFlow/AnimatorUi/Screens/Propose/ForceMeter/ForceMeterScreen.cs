using System;
using System.Collections.Generic;
using DG.Tweening;
using Modules.General.Abstraction;
using Modules.Sound;
using Drawmasters.Advertising;
using Drawmasters.Effects;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using Modules.Analytics;
using Modules.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;

namespace Drawmasters.Ui
{
    public class ForceMeterScreen : RewardReceiveScreen
    {
        #region Fields

        public static event Action<RewardData[]> OnRewardSetted;
        public static event Action<int> OnShouldPlayHitAnimation; // iteration index

        [SerializeField] private TMP_Text skipText = default;
        [SerializeField] private Button nextButton = default;

        [Header("Button")]
        [SerializeField] private Button hitButton = default;
        [SerializeField] private RewardedVideoButton hitVideoButton = default;
        [SerializeField] private CanvasGroup hitButtonCanvasGroup = default;
        [SerializeField] private GameObject adsIcon = default;
        [SerializeField] private Animator hitButtonAnimator = default;
        [SerializeField] private TMP_Text hitButtonText = default;
        [SerializeField] private IdleEffect hitButtonIdleEffect = default;

        [Header("Button  Animations")]
        [SerializeField] private FactorAnimation hitButtonBackAnimation = default;


        [Header("Proposal reward scene")]
        [SerializeField] private IdleEffect[] rewardDataSceneIdleEffects = default;


        private RewardData[] currentReward;
        private List<RewardData> restRewardToReceive;

        private ForcemeterRewardElement[] elements;

        private ForceMeterUiSettings uiSettings;
        
        private bool wasVideoShown;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.ForceMeter;


        private RewardData NextRewardDataToReceive =>
            restRewardToReceive.First();


        private bool IsRewardDataScene =>
            GameServices.Instance.LevelEnvironment.Context.IsProposalSceneFromRewardData;


        #endregion



        #region Methods

        public void SetReward(RewardData[] _currentReward)
        {
            currentReward = _currentReward;
            restRewardToReceive = new List<RewardData>(currentReward);

            if (skipText.TryGetComponent(out Localize skipTextLoc))
            {
                skipTextLoc.SetTerm(IngameData.Settings.commonRewardSettings.nothingReceivedSkipKey);
            }

            RefreshHitButtonVisual(GetActualIterationIndex());

            OnRewardSetted?.Invoke(currentReward);
        }

        #endregion



        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            uiSettings = IngameData.Settings.forceMeterUiSettings;

            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.RefreshCurrencyVisual(0.0f);

            ForcemeterAnimationComponent.OnProgressFinishFill += ForcemeterAnimationComponent_OnProgressFinishFill;
            ForcemeterRewardComponent.OnRewardApplied += ForcemeterRewardComponent_OnRewardApplied;
            ForcemeterAnimationComponent.OnShouldApplyReward += ForcemeterAnimationComponent_OnShouldApplyReward;

            LocalizationManager.OnLocalizeEvent += LocalizationManager_OnLocalizeEvent;

            hitButtonIdleEffect.CreateAndPlayEffect();

            hitVideoButton.Initialize(AdsVideoPlaceKeys.Forcemeter);
            hitVideoButton.OnVideoShowEnded += HitVideoButton_OnVideoShowEnded;
        }


        public override void Deinitialize()
        {
            ForcemeterAnimationComponent.OnProgressFinishFill -= ForcemeterAnimationComponent_OnProgressFinishFill;
            ForcemeterRewardComponent.OnRewardApplied -= ForcemeterRewardComponent_OnRewardApplied;
            ForcemeterAnimationComponent.OnShouldApplyReward -= ForcemeterAnimationComponent_OnShouldApplyReward;

            hitVideoButton.Deinitialize();
            hitVideoButton.OnVideoShowEnded -= HitVideoButton_OnVideoShowEnded;

            LocalizationManager.OnLocalizeEvent -= LocalizationManager_OnLocalizeEvent;

            currentReward = null;
            uiHudTop.DeinitializeCurrencyRefresh();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(this);

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            nextButton.interactable = false;
            skipText.alpha = default;

            EnableRewardSceneEffects();
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SHOPENTER);
        }


        public override void Hide()
        {
            DisableRewardSceneEffects();
            hitButtonIdleEffect.StopEffect();
            DeinitializeButtons();

            base.Hide();
        }


        public override void InitializeButtons()
        {
            hitButton.onClick.AddListener(HitButton_OnClick);
            nextButton.onClick.AddListener(Hide);
        }


        public override void DeinitializeButtons()
        {
            hitButton.onClick.RemoveListener(HitButton_OnClick);
            nextButton.onClick.RemoveListener(Hide);

            hitVideoButton.DeinitializeButtons();
        }


        private void RefreshHitButtonVisual(int iterationIndex)
        {
            bool isAnyRewardRest = iterationIndex < currentReward.Length;

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIDynamometerButtonHitChange, parent: hitButton.transform, transformMode: TransformMode.Local);

            if (isAnyRewardRest)
            {
                RefreshText(iterationIndex);
                string buttonTrigger = uiSettings.FindButtonAnimationTriggerKey(iterationIndex);
                hitButtonAnimator.SetTrigger(buttonTrigger);
            }
        }


        private void RefreshText(int iterationIndex)
        {
            string buttonKey = uiSettings.FindButtonKey(iterationIndex);

            hitButtonText.text = LocalizationManager.GetTermTranslation(buttonKey);

            bool isAnyRewardRest = currentReward != null && iterationIndex < currentReward.Length;
            if (isAnyRewardRest)
            {
                bool shouldShowAdsIcon = !IsRewardDataScene && currentReward[iterationIndex].receiveType == RewardDataReceiveType.Video;
                CommonUtility.SetObjectActive(adsIcon, shouldShowAdsIcon);
            }
        }


        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            Vector3 result = default;

            ForcemeterRewardElement uiForceMeterElement = Array.Find(elements, e => e.RewardData == rewardData);

            if (uiForceMeterElement != null)
            {
                result = TransformUtility.WorldToUiPosition(uiForceMeterElement.IconCurrencyPosition);
            }

            return result;
        }


        private void ShowNextButton()
        {
            nextButton.interactable = false;
            
            FactorAnimation animationSettings = 
                IngameData.Settings.commonRewardSettings.ForcemeterSkipRootAlphaAnimation; 

            animationSettings.Play(value => 
                skipText.alpha = value, this, () => nextButton.interactable = true);
        }


        private void HideNextButton()
        {
            nextButton.interactable = false;

            if (!IsRewardDataScene)
            {
                FactorAnimation animationSettings = 
                        IngameData.Settings.commonRewardSettings.ForcemeterSkipRootAlphaAnimation;
                    
                animationSettings.Play(value => skipText.alpha = value, this, null, true);
            }
        }


        private void PlayToDisableHitButton()
        {
            hitButton.interactable = false;

            int iterationIndex = GetActualIterationIndex();
            
            hitButtonBackAnimation.Play(value => hitButtonCanvasGroup.alpha = value, this,
                () => RefreshText(iterationIndex + 1), true);

            hitButtonIdleEffect.StopEffect();
        }


        private void PlayToEnableHitButton()
        {
            hitButtonBackAnimation.Play(value => hitButtonCanvasGroup.alpha = value, this,
                () => hitButton.interactable = true);

            hitButtonIdleEffect.CreateAndPlayEffect();
        }


        private int GetActualIterationIndex() =>
            IsRewardDataScene ? currentReward.Length - 1 : currentReward.Length - restRewardToReceive.Count;
        
        #endregion



        #region Events handlers

        private void HitVideoButton_OnVideoShowEnded(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                wasVideoShown = true;
                OnShouldReceiveReward();
            }
        }


        private void HitButton_OnClick()
        {
            hitButton.onClick.RemoveListener(HitButton_OnClick);
            hitVideoButton.InitializeButtons();

            OnShouldReceiveReward();
        }


        private void ForcemeterRewardComponent_OnRewardApplied(ForcemeterRewardElement[] rewardElements) =>
            elements = rewardElements;


        private void ForcemeterAnimationComponent_OnShouldApplyReward(int iterationIndex)
        {
            if (!IsRewardDataScene) // cuz we apply all reward after finish filling
            {
                ReceiveNextReward();
            }
        }


        private void OnShouldReceiveReward()
        {
            EventSystemController.SetSystemEnabled(false, this);

            int iterationIndex = GetActualIterationIndex();
            RefreshHitButtonVisual(iterationIndex + 1);
            RefreshText(iterationIndex);

            CameraShakeSettings.Shake shakeData = uiSettings.FindShakeAnimation(iterationIndex);
            IngameCamera.Instance.Shake(shakeData);

            if (iterationIndex > 0)
            {
                HideNextButton();
            }

            PlayToDisableHitButton();

            OnShouldPlayHitAnimation?.Invoke(iterationIndex);
        }


        private void ForcemeterAnimationComponent_OnProgressFinishFill(int iterationIndex)
        {
            if (IsRewardDataScene)
            {
                EventSystemController.SetSystemEnabled(true, this);

                DisableRewardSceneEffects();
                OnShouldApplyReward(restRewardToReceive.ToArray(), Hide);
            }
            else
            {
                OnProgressFinishFill();
            }
        }


        private void ReceiveNextReward()
        {
            EventSystemController.SetSystemEnabled(true, this);

            RewardData receivedRewardData = NextRewardDataToReceive;
            restRewardToReceive.Remove(receivedRewardData);

            bool isAnyRewardRest = restRewardToReceive.Count > 0;

            if (!isAnyRewardRest)
            {
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                    OnShouldApplyReward(receivedRewardData, Hide), uiSettings.skinRewardOpenDelay);
            }
            else
            {
                OnShouldApplyReward(receivedRewardData);
            }

            if (wasVideoShown)
            {
                CommonEvents.SendAdVideoReward(hitVideoButton.Placement);
            }

            wasVideoShown = false;
        }


        private void OnProgressFinishFill()
        {
            EventSystemController.SetSystemEnabled(true, this);

            bool isAnyRewardRest = restRewardToReceive.Count > 0;

            if (skipText.TryGetComponent(out Localize skipTextLoc))
            {
                skipTextLoc.SetTerm(IngameData.Settings.commonRewardSettings.somethingReceivedSkipKey);
            }

            if (isAnyRewardRest)
            {
                ShowNextButton();
                PlayToEnableHitButton();
            }
        }


        private void EnableRewardSceneEffects()
        {
            if (!IsRewardDataScene)
            {
                return;
            }

            foreach (var e in rewardDataSceneIdleEffects)
            {
                e.CreateAndPlayEffect();
            }
        }


        private void DisableRewardSceneEffects()
        {
            foreach (var e in rewardDataSceneIdleEffects)
            {
                e.StopEffect();
            }
        }


        private void LocalizationManager_OnLocalizeEvent() =>
           RefreshText(GetActualIterationIndex());

        #endregion
    }
}
