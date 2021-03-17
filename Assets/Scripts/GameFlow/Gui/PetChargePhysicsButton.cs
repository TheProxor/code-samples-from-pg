using System;
using UnityEngine;
using Drawmasters.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.Advertising;
using Drawmasters.Pets;
using Drawmasters.Effects;
using Modules.General;
using Modules.Advertising;
using Modules.General.Abstraction;
using DG.Tweening;


namespace Drawmasters.Ui
{
    public class PetChargePhysicsButton : IngameTouchMonitor
    {
        #region Helpers

        private static class AnimationHashs
        {
            public static int ShowAnimationHash = Animator.StringToHash(AnimationKeys.Screen.Show);

            public static int HideAnimationHash = Animator.StringToHash(AnimationKeys.Screen.Hide);
        }

        #endregion



        #region Fields

        public event Action OnEnableButton;
        public event Action OnDisableButton;

        [Header("Common")]
        [SerializeField] private GameObject buttonVisualRoot = default;
        [SerializeField] private float buttonDisableDelay = default;
        [SerializeField] private Animator buttonAnimator = default;
        [SerializeField] private IngameTouchMonitor petButton;

        [Header("Loading state")]
        [SerializeField] private float rotationDuration = default;
        [SerializeField] private ColorAnimation loadingStateBackgroundFadeAnimation = default;
        [SerializeField] private ColorAnimation loadingStateCircleFadeAnimation = default;
        [SerializeField] private SpriteRenderer loadingStateBackgroundSprite = default;
        [SerializeField] private SpriteRenderer loadingStateCircleSprite = default;


        private bool isEnabled;
        private bool isLoadingState;

        private PetsChargeController petsChargeController;
        private PetsTutorialController petsTutorialController;

        #endregion



        #region Properties

        private bool IsPetSleep =>
            !petsChargeController.IsCurrentPetCharged &&
            !petsTutorialController.ShouldReckonPetCharged;


        private bool CanEnableButtonOnTapPet =>
            IsPetSleep && IsMainMenuScreenActive;


        private bool IsMainMenuScreenActive =>
            UiScreenManager.Instance.IsScreenActive(ScreenType.MainMenu);


        private bool IsOutfitsScreenActive =>
            UiScreenManager.Instance.IsScreenActive(ScreenType.OutfitsSkinScreen);

        #endregion



        #region Class Lifecycle

        public void Initialize()
        {
            isEnabled = false;

            petsChargeController = GameServices.Instance.PetsService.ChargeController;
            petsTutorialController = GameServices.Instance.PetsService.TutorialController;

            OnTap += PhysicsButton_OnMouseButtonDown;

            petButton.OnTap += IngameTouchMonitor_OnAnyTap;

            AnimatorScreen.OnScreenShow += AnimatorScreen_OnScreenShow;
            AnimatorScreen.OnScreenHide += AnimatorScreen_OnScreenHide;

            loadingStateBackgroundSprite.color = loadingStateBackgroundSprite.color.SetA(0.0f);
            loadingStateCircleSprite.color = loadingStateCircleSprite.color.SetA(0.0f);

            InitializeButton();
        }


        public void Deinitialize()
        {
            OnTap -= PhysicsButton_OnMouseButtonDown;

            petButton.OnTap -= IngameTouchMonitor_OnAnyTap;

            AnimatorScreen.OnScreenShow -= AnimatorScreen_OnScreenShow;
            AnimatorScreen.OnScreenHide -= AnimatorScreen_OnScreenHide;

            DOTween.Kill(this);

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Methods

        private void InitializeButton()
        {
            if (IsOutfitsScreenActive)
            {
                if (petsChargeController.IsCurrentPetCharged)
                {
                    DisableButton();
                }
                else
                {
                    EnableButton();
                }
            }
            else
            {
                DisableButton();
            }
        }

        private void ShowAds()
        {
            AdvertisingManager.Instance.TryShowAdByModule(AdModule.RewardedVideo, AdsVideoPlaceKeys.PetCharge, result =>
            {
                if (this.IsNull())
                {
                    CustomDebug.Log("Rewarded video button is null on showed callback");
                    return;
                }

                switch (result)
                {
                    case AdActionResultType.Success:
                        Charge();
                        DisableButton();
                        break;

                    case AdActionResultType.NotAvailable:
                        ShowLoadingState();
                        break;

                    case AdActionResultType.NoInternet:
                        UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                        break;

                    default:
                        break;
                }
            });
        }


        private void Charge() =>
            petsChargeController.ChargeImmediately();


        private void EnableButton()
        {
            if (!IsPetSleep)
            {
                return;
            }

            SetButtonEnabled(true);
            OnEnableButton?.Invoke();
        }


        private void DisableButton()
        {
            SetButtonEnabled(false);
            OnDisableButton?.Invoke();
        }


        private void SetButtonEnabled(bool enabled)
        {
            if (buttonAnimator == null || isEnabled == enabled)
            {
                return;
            }

            Scheduler.Instance.UnscheduleMethod(this, DisableButton);

            Lock(!enabled);

            buttonAnimator.ResetTrigger(AnimationKeys.Screen.Show);
            buttonAnimator.ResetTrigger(AnimationKeys.Screen.Hide);
            buttonAnimator.SetTrigger(enabled ? AnimationKeys.Screen.Show : AnimationKeys.Screen.Hide);

            if (enabled)
            {
                buttonAnimator.Play(AnimationHashs.ShowAnimationHash);

                if (CanEnableButtonOnTapPet)
                {
                    Scheduler.Instance.CallMethodWithDelay(this, DisableButton, buttonDisableDelay);
                }
            }
            else
            {
                EffectManager.Instance.CreateSystem(EffectKeys.FxGUISleepBubbleExplode, false, transform.position, default, transform);
                buttonAnimator.Play(AnimationHashs.HideAnimationHash);

                if (isLoadingState)
                {
                    FadeLoadingStateSprites(false);
                }
            }

            isLoadingState = false;
            isEnabled = enabled;
        }


        private void ShowLoadingState()
        {
            DOTween.Kill(this);

            FadeLoadingStateSprites(true);

            loadingStateCircleSprite.transform
                .DORotate(new Vector3(0.0f, 0.0f, 360.0f), rotationDuration, RotateMode.FastBeyond360)
                .SetId(this)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);

            isLoadingState = true;
        }


        private void FadeLoadingStateSprites(bool enabled)
        {
            loadingStateBackgroundFadeAnimation.Play((value) => loadingStateBackgroundSprite.color = loadingStateBackgroundSprite.color.SetA(value.a), 
                                                     this,
                                                     isReversed: !enabled);

            loadingStateCircleFadeAnimation.Play((value) => loadingStateCircleSprite.color = loadingStateCircleSprite.color.SetA(value.a), 
                                                  this,
                                                  isReversed: !enabled);
        }

        #endregion



        #region Event Handlers

        private void PhysicsButton_OnMouseButtonDown() =>
            ShowAds();


        private void IngameTouchMonitor_OnAnyTap()
        {
            if (CanEnableButtonOnTapPet)
            {
                EnableButton();
            }
        }


        private void AnimatorScreen_OnScreenShow(ScreenType screenType)
        {
            if (screenType == ScreenType.OutfitsSkinScreen)
            {
                Scheduler.Instance.UnscheduleMethod(this, DisableButton);

                EnableButton();
            }
        }


        private void AnimatorScreen_OnScreenHide(ScreenType screenType)
        {
            if (screenType == ScreenType.OutfitsSkinScreen)
            {
                DisableButton();
            }
        }

        #endregion
    }
}