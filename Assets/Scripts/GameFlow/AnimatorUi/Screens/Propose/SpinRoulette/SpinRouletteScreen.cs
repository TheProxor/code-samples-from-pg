using TMPro;
using System;
using System.Linq;
using DG.Tweening;
using Modules.Sound;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Vibration;
using Drawmasters.Effects;
using Drawmasters.Advertising;
using Drawmasters.Helpers;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Modules.General.Abstraction;
using Modules.Analytics;
using Modules.General;
using Sirenix.OdinInspector;
using System.Collections.Generic;


namespace Drawmasters.Ui
{
    public class SpinRouletteScreen : RewardReceiveScreen
    {
        #region Fields

        [SerializeField] [Required] private Button closeButton = default;
        [SerializeField] private GameObject reloadTimer = default;

        [SerializeField] private Image rouletteWheelSprite = default;
        [SerializeField] [Required] private SpineRouletteElement[] elements = default;

        [SerializeField] [Required] private TMP_Text timeForNextRefreshText = default;
        [SerializeField] [Required] private TMP_Text gemsForSpinText = default;

        [SerializeField] [Required] private CanvasGroup[] buttonsCanvasGroup = default;

        [SerializeField] [Required] private Button freeSpinButton = default;

        [SerializeField] [Required] private Button currencySpinButton = default;
         
        [SerializeField] [Required] private RewardedVideoButton adsSpinButton = default;

        [SerializeField] private IdleEffect idleShineEffect = default;
        [SerializeField] private IdleEffect idleShineFrameEffect = default;

        [Header("Rotate animation")]
        [SerializeField] [Required] private Transform rotateTransform = default;
        
        [SerializeField] private IdleEffect idleSpinEffect = default;
        [SerializeField] private FactorAnimation idleSpinEffectFadeAnimation = default;
        [SerializeField] private FactorAnimation idleSpinEffectAppearAnimation = default;

        private RewardData[] currentReward;
        private SpinRouletteController controller;

        private SpineRouletteElement lastWinElement;

        private object spinId;

        private bool wasFreeSpinUsed;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.SpinRoulette;



        public Sprite RouletteWheelSprite
        {
            get => rouletteWheelSprite.sprite;
            set => rouletteWheelSprite.sprite = value;
        }
        
        #endregion



        #region Methods

        public void SetReward(RewardData[] _currentReward, float delay) =>
            Scheduler.Instance.CallMethodWithDelay(this, () => SetReward(_currentReward), delay);


        public void SetReward(RewardData[] _currentReward)
        {
            currentReward = _currentReward;

            for (int i = 0; i < elements.Length || i < currentReward.Length; i++)
            {
                elements[i].SetupReward(currentReward[i]);
            }

            RefreshVisual();
        }


        private void RefreshVisual()
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].RefreshVisual();

                Color backColor = controller.FindSegmentColor(i);
                elements[i].SetBackColor(backColor);
            }
        }

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            controller = GameServices.Instance.ProposalService.SpinRouletteController;
            
            spinId = Guid.NewGuid();

            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.RefreshCurrencyVisual(0.0f);

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdateMonitorAnimationTimer;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdateRefreshTimer;
            controller.OnRewardRefreshed += Controller_OnRewardRefreshed;
            controller.OnPreRewardRefresh += Controller_OnPreRewardRefresh;

            idleShineEffect.CreateAndPlayEffect();
            idleShineFrameEffect.CreateAndPlayEffect();

            gemsForSpinText.text = controller.Settings.gemsForSpin.ToShortFormat();

            adsSpinButton.Initialize(AdsVideoPlaceKeys.SpinRoulette);
            adsSpinButton.OnVideoShowEnded += AdsSpinButton_OnVideoShowEnded;
        }


        public override void Deinitialize()
        {
            uiHudTop.DeinitializeCurrencyRefresh();

            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateMonitorAnimationTimer;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateRefreshTimer;
            controller.OnRewardRefreshed -= Controller_OnRewardRefreshed;
            controller.OnPreRewardRefresh -= Controller_OnPreRewardRefresh;

            DOTween.Kill(spinId);
            DOTween.Kill(this);

            Scheduler.Instance.UnscheduleAllMethodForTarget(spinId);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            currentReward = null;
            foreach (var element in elements)

            {
                element.Deinitialize();
            }

            adsSpinButton.Deinitialize();
            adsSpinButton.DeinitializeButtons();

            adsSpinButton.OnVideoShowEnded -= AdsSpinButton_OnVideoShowEnded;

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            var mainMenuScreen = UiScreenManager.Instance.LoadedScreen<UiMainMenuScreen>(ScreenType.MainMenu);
            if (!mainMenuScreen.IsNull())
            {
                mainMenuScreen.HideUiHudTop();
            }

            CommonUtility.SetObjectActive(closeButton.gameObject, !controller.ShouldGetOnlyOneReward);
            RefreshUi();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SHOPENTER);
        }


        public override void Hide()
        {
            var mainMenuScreen = UiScreenManager.Instance.LoadedScreen<UiMainMenuScreen>(ScreenType.MainMenu);
            if (!mainMenuScreen.IsNull())
            {
                mainMenuScreen.ShowUiHudTop();
            }

            idleShineEffect.StopEffect();
            idleShineFrameEffect.StopEffect();
            idleSpinEffect.StopEffect();

            base.Hide();
        }


        public override void InitializeButtons()
        {
            adsSpinButton.InitializeButtons();

            closeButton.onClick.AddListener(Hide);
            freeSpinButton.onClick.AddListener(FreeSpinButton_OnClick);
            currencySpinButton.onClick.AddListener(CurrencySpinButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            adsSpinButton.DeinitializeButtons();

            closeButton.onClick.RemoveListener(Hide);

            freeSpinButton.onClick.RemoveListener(FreeSpinButton_OnClick);
            currencySpinButton.onClick.RemoveListener(CurrencySpinButton_OnClick);
        }


        // also used in Unity Animation Event
        private void RefreshUi()
        {
            CommonUtility.SetObjectActive(reloadTimer, !controller.ShouldGetOnlyOneReward);
            CommonUtility.SetObjectActive(freeSpinButton.gameObject, controller.ShouldGetOnlyOneReward || controller.IsFreeSpinAvailable);
            CommonUtility.SetObjectActive(currencySpinButton.gameObject, !controller.ShouldGetOnlyOneReward && !controller.IsFreeSpinAvailable);
            CommonUtility.SetObjectActive(adsSpinButton.gameObject, !controller.ShouldGetOnlyOneReward && !controller.IsFreeSpinAvailable);
        }


        private void SpinForReward(bool isFreeSpin, Action<RewardData> callback = default)
        {
            DeinitializeButtons();

            controller.Settings.buttonsBlendAnimation.Play(e =>
            {
                foreach (var cg in buttonsCanvasGroup)
                {
                    cg.alpha = e;
                }
            }, this, isReversed: true);

            HideElementWinEffect();

            SoundManager.Instance.PlaySound(AudioKeys.Ui.WHEEL_START);
            SoundManager.Instance.PlaySound(AudioKeys.Ui.WHEEL_WIN_FLYING_COLLECT);

            RewardData[] restAvailableReward = currentReward.Where(e => e.IsAvailableForRewardPack).ToArray();
            RewardData receivedRewardData = controller.Settings.GetRandomSpinReward(controller.ShowsCount - 1, restAvailableReward, isFreeSpin);

            int elementIndexToSpin = Array.FindIndex(elements, e => e.RewardData == receivedRewardData);

            if (elementIndexToSpin != -1)
            {
                lastWinElement = elements[elementIndexToSpin];
            }

            Spin(elementIndexToSpin, controller.Settings.rewardSpinSettings, OnSpinned);


            void OnSpinned()
            {
                VibrationManager.Play(IngameVibrationType.RouletteEnd);
                SoundManager.Instance.PlaySound(AudioKeys.Ui.WHEEL_SECTOR_WIN);

                if (lastWinElement != null)
                {
                    lastWinElement.ShowWinEffect();
                }

                if (receivedRewardData != null)
                {
                    OnShouldApplyReward(receivedRewardData, () =>
                    {
                        if (controller.ShouldGetOnlyOneReward)
                        {
                            float delay = receivedRewardData.Type == RewardType.Currency ? controller.Settings.hideRewardSceneDelay : 0.0f;
                            Scheduler.Instance.CallMethodWithDelay(this, Hide, delay);
                        }
                    });
                    SoundManager.Instance.PlayOneShot(AudioKeys.Ui.WHEEL_WIN_FLYING_COLLECT);
                }

                RefreshVisual();

                if (!controller.ShouldGetOnlyOneReward)
                {
                    InitializeButtons();

                    controller.Settings.buttonsBlendAnimation.Play(e =>
                    {
                        foreach (var cg in buttonsCanvasGroup)
                        {
                            cg.alpha = e;
                        }
                    }, this);

                    callback?.Invoke(receivedRewardData);
                }
            }
        }


        private void SpinForRefresh()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateMonitorAnimationTimer;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdateMonitorAnimationTimer;

            HideElementWinEffect();

            controller.Settings.buttonsBlendAnimation.Play(e =>
            {
                foreach (var cg in buttonsCanvasGroup)
                {
                    cg.alpha = e;
                }
            }, this, isReversed: true);

            int elementIndexToSpin = lastWinElement == null ? 0 : Array.FindIndex(elements, e => e == lastWinElement);
            Spin(elementIndexToSpin, controller.Settings.refreshSpinSettings, OnSpinned);

            void OnSpinned()
            {
                if (wasFreeSpinUsed)
                {
                    mainAnimator.SetTrigger(AnimationKeys.SpinRouletteScreen.ChangeSpinButtonsToFree);
                }

                controller.Settings.buttonsBlendAnimation.Play(e =>
                {
                    foreach (var cg in buttonsCanvasGroup)
                    {
                        cg.alpha = e;
                    }
                }, this);

                controller.OnRewardRefreshed += Controller_OnRewardRefreshed;
                controller.OnPreRewardRefresh += Controller_OnPreRewardRefresh;
            }

        }


        private void Spin(int elementIndexToSpin, SpinRouletteSettings.SpinSettings spinSettings, Action callback)
        {
            int killedTweenersCount = DOTween.Kill(spinId);
            if (killedTweenersCount > 0)
            {
                CustomDebug.Log("Possible bug can be here.");
            }
            
            idleSpinEffect.CreateAndPlayEffect();
            idleSpinEffect.SetAlpha(0.0f);

            idleSpinEffectAppearAnimation.duration = spinSettings.fxShowDuration;
            idleSpinEffectAppearAnimation.delay = spinSettings.fxShowDelay;
            idleSpinEffectAppearAnimation.Play(value => idleSpinEffect.SetAlpha(value), this);

            idleSpinEffectFadeAnimation.duration = spinSettings.fxHideDuration;
            idleSpinEffectFadeAnimation.delay = spinSettings.fxHideDelay;
            idleSpinEffectFadeAnimation.Play(value => idleSpinEffect.SetAlpha(value), this, () => idleSpinEffect.StopEffect());

            float rotateAngle = controller.AngleForSegment(elementIndexToSpin);
            
            rotateTransform
                .DORotate(Vector3.zero.SetZ(rotateAngle), 
                    spinSettings.rotateDuration, 
                    RotateMode.FastBeyond360)
                .SetId(spinId)
                .OnComplete(() =>
                {
                    callback();
                })
                .SetEase(spinSettings.spinCurve);
        }


        private void HideElementWinEffect()
        {
            if (lastWinElement != null)
            {
                lastWinElement.HideWinEffect();
            }
        }

        // Unity Animator
        private void StopElementWinEffect()
        {
            if (lastWinElement != null)
            {
                lastWinElement.StopWinEffect();
            }
        }

        #endregion



        #region Events handlers

        private void FreeSpinButton_OnClick()
        {
            controller.MarkSpinedFree();

            SpinForReward(true, data =>
            {
                CommonUtility.SetObjectActive(freeSpinButton.gameObject, true);
                CommonUtility.SetObjectActive(currencySpinButton.gameObject, true);
                CommonUtility.SetObjectActive(adsSpinButton.gameObject, true);

                mainAnimator.SetTrigger(AnimationKeys.SpinRouletteScreen.ChangeSpinButtonsFromFree);
            });
        }


        private void CurrencySpinButton_OnClick()
        {
            SpinRouletteSettings settings = IngameData.Settings.spinRouletteSettings;
            IProposable proposable = new CurrencyProposal((settings.gemsForSpin, CurrencyType.Premium));

            proposable.Propose((result) =>
            {
                if (result)
                {
                    adsSpinButton.CancelAdShowRequest();
                    SpinForReward(false);
                }
            });
        }



        private void AdsSpinButton_OnVideoShowEnded(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                OnReceivedFromVideo();
            }
        }


        private void OnReceivedFromVideo() =>
            SpinForReward(false, data =>
            {
                CommonEvents.SendAdVideoReward(adsSpinButton.Placement);
            });


        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            SpineRouletteElement foundElement = Array.Find(elements, e => e.RewardData == rewardData);
            return foundElement == null ? default : foundElement.IconCurrencyTransform.position;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdateMonitorAnimationTimer(float deltaTime)
        {
            if (controller.ShouldPlayTimerAnimation)
            {
                MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdateMonitorAnimationTimer;
                
                controller.Settings.timerBounceAnimation.Play(value => 
                    timeForNextRefreshText.transform.localScale = value, this);
            }
        }


        private void MonoBehaviourLifecycle_OnUpdateRefreshTimer(float deltaTime) =>
            timeForNextRefreshText.text = controller.TimeUi;


        private void Controller_OnRewardRefreshed()
        {
            DOTween.Kill(this);
            timeForNextRefreshText.transform.localScale = controller.Settings.timerBounceAnimation.beginValue;

            controller.OnRewardRefreshed -= Controller_OnRewardRefreshed;
            controller.OnPreRewardRefresh -= Controller_OnPreRewardRefresh;

            List<Tween> list = DOTween.TweensById(spinId);

            if (list.IsNullOrEmpty())
            {
                SpinForRefresh();
            }
            else
            {
                Scheduler.Instance.UnscheduleAllMethodForTarget(spinId);

                list.First().onComplete += () =>
                {
                    Scheduler.Instance.CallMethodWithDelay(spinId, 
                        SpinForRefresh, 
                        controller.Settings.refreshDelayForSpinningState);
                };
            }
        }


        private void Controller_OnPreRewardRefresh() =>
            wasFreeSpinUsed = !controller.IsFreeSpinAvailable;

        #endregion
    }
}
