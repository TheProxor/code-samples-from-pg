using System;
using System.Collections.Generic;
using DG.Tweening;
using Modules.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;
using System.Collections;
using Modules.Sound;


namespace Drawmasters.Ui
{
    public class RateUsFeedbackPopupScreen : AnimatorScreen
    {
        #region Nested types
         
        #pragma warning disable 0649

        [Serializable]
        private struct StarButton
        {
            public Button button;
            public Animator bounceAnimator;
            public Image fillStar;
            [Enum(typeof(AudioKeys.Ui))]
            public string audioKey;
        }


        [Serializable]
        private struct StateButton
        {
            public Button activeButton;
            public GameObject inactiveButton;

            public void SetActive(bool isActive)
            {
                if (activeButton != null)
                {
                    activeButton.gameObject.SetActive(isActive);
                }

                if (inactiveButton != null)
                {
                    inactiveButton.SetActive(!isActive);
                }
            }

            public void DisableAll()
            {
                if (activeButton != null)
                {
                    activeButton.gameObject.SetActive(false);
                }

                if (inactiveButton != null)
                {
                    inactiveButton.SetActive(false);
                }
            }
        }

        #pragma warning restore 0649

        #endregion



        #region Fields

        public event Action OnRated;
        public event Action OnShouldPlayFxs;

        private const string SwitchToRewardAnimatorKey = "SwitchToReward";
        private const float MoveContainerDuration = 0.5f;
        private const int CountRateStarsToFeedback = 3;

        [SerializeField] private Button closePopupButton = default;
        [SerializeField] private Button nextStateButton = default;

        [Header("Rate")]
        [SerializeField] private StateButton rateStateButton = default;
        [SerializeField] private List<StarButton> starButtons = default;

        [Header("Feedback")]
        [SerializeField] private GameObject caret = default;
        [SerializeField] private TMP_InputField inputField = default;
        [SerializeField] private Button openInStoreButton = default;
        [SerializeField] private StateButton sendFeedbackStateButton = default;

        [Header("Move container roots")]
        [SerializeField] private Transform containerRoot = default;
        [SerializeField] private Transform centerContainerRoot = default;
        [SerializeField] private Transform upContainerRoot = default;

        [Header("Custom Behaviour")]
        [SerializeField] private RateUsRewardBehaviour.Data commonData = default;

        private Tweener moveTween;

        private int currentRateStars;
        private string previousText;

        private IRateUsService rateUsService;
        private IUiBehaviour uiCustomBehaviour;

        #endregion


        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.RateUsFeedbackScreen;

        #endregion



        #region Overrided Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            rateUsService = GameServices.Instance.RateUsService;

            LLApplicationStateRegister.OnApplicationEnteredBackground +=
                LlApplicationStateRegister_OnApplicationEnteredBackground;

            currentRateStars = 0;

            SetCountStars(currentRateStars - 1);

            rateStateButton.SetActive(currentRateStars != 0);
            sendFeedbackStateButton.SetActive(!string.IsNullOrEmpty(inputField.text));
            CommonUtility.SetObjectActive(commonData.loaderButton, false);

            uiCustomBehaviour = new RateUsRewardBehaviour(commonData, this, rateUsService);
            uiCustomBehaviour.Enable();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.RATEUS_POPUP_APPEAR);
        }


        public override void Deinitialize()
        {
            uiCustomBehaviour.Disable();
            uiCustomBehaviour.Deinitialize();

            LLApplicationStateRegister.OnApplicationEnteredBackground -=
                LlApplicationStateRegister_OnApplicationEnteredBackground;

            base.Deinitialize();

        }


        public override void InitializeButtons()
        {
            for (int i = 0; i < starButtons.Count; i++)
            {
                int indexStarButton = i;
                starButtons[i].button.onClick.AddListener(() => StarButton_OnClick(indexStarButton));
            }

            uiCustomBehaviour.InitializeButtons();

            closePopupButton.onClick.AddListener(Hide);
            rateStateButton.activeButton.onClick.AddListener(RateButton_OnClick);
            openInStoreButton.onClick.AddListener(OpenInStoreButton_OnClick);
            sendFeedbackStateButton.activeButton.onClick.AddListener(SendFeedbackButton_OnClick);
            nextStateButton.onClick.AddListener(Hide);

            inputField.onValueChanged.AddListener(InputField_OnValueChanged);
            inputField.onSelect.AddListener(InputField_Select);
            inputField.onDeselect.AddListener(InputField_Deselect);
        }


        public override void DeinitializeButtons()
        {
            for (int i = 0; i < starButtons.Count; i++)
            {
                starButtons[i].button.onClick.RemoveAllListeners();
            }

            uiCustomBehaviour.DeinitializeButtons();

            closePopupButton.onClick.RemoveListener(Hide);
            rateStateButton.activeButton.onClick.RemoveListener(RateButton_OnClick);
            openInStoreButton.onClick.RemoveListener(OpenInStoreButton_OnClick);
            sendFeedbackStateButton.activeButton.onClick.RemoveListener(SendFeedbackButton_OnClick);
            nextStateButton.onClick.RemoveListener(Hide);

            inputField.onValueChanged.RemoveListener(InputField_OnValueChanged);
            inputField.onSelect.RemoveListener(InputField_Select);
            inputField.onDeselect.RemoveListener(InputField_Deselect);
        }

        #endregion



        #region Private methods

        private void SetCountStars(int indexStarButton)
        {
            for (int i = starButtons.Count - 1; i >= 0; i--)
            {
                starButtons[i].fillStar.gameObject.SetActive(i <= indexStarButton);
            }
        }


        private void DisableStarBouncing()
        {
            for (int i = 0; i < starButtons.Count; i++)
            {
                starButtons[i].bounceAnimator.enabled = false;
            }
        }


        private void MoveContainerToPosition(Vector3 position, float duration)
        {
            if (moveTween != null)
            {
                moveTween.ChangeEndValue(position, moveTween.Elapsed(), true);
            }
            else
            {
                moveTween = containerRoot.DOMove(position, duration).OnComplete(() => moveTween = null);
            }
        }


        private void OnRatedAction()
        {
            if (rateUsService.IsRewardAvailable)
            {
                rateStateButton.DisableAll();
                mainAnimator.SetTrigger(SwitchToRewardAnimatorKey);
                SoundManager.Instance.PlayOneShot(AudioKeys.Ui.RATEUS_THANK_YOU);
            }

            OnRated?.Invoke();
        }


        // Called from unity animator
        private void PlayRatedFxs() =>
            OnShouldPlayFxs?.Invoke();

        #endregion



        #region Events handlers

        private void LlApplicationStateRegister_OnApplicationEnteredBackground(bool isEntered)
        {
            if (!isEntered && rateUsService.IsRated)
            {
                StartCoroutine(ProceedRoutine());
            }


            IEnumerator ProceedRoutine()
            {
                yield return new WaitForEndOfFrame();

                OnRatedAction();
            }
        }


        private void StarButton_OnClick(int indexButtonSender)
        {
            if (currentRateStars == 0)
            {
                DisableStarBouncing();
                mainAnimator.SetTrigger(AnimationKeys.RateFeedback.StarPressed);
            }

            SetCountStars(indexButtonSender);
            currentRateStars = indexButtonSender + 1;
            rateStateButton.SetActive(currentRateStars != 0);
            SoundManager.Instance.PlayOneShot(starButtons[indexButtonSender].audioKey);
        }


        private void RateButton_OnClick()
        {
            if (currentRateStars > CountRateStarsToFeedback)
            {
                GameServices.Instance.ProposalService.RateUsProposal.RateRequset();
#if UNITY_EDITOR
                OnRatedAction();
#endif
            }
            else
            {
                DeinitializeButtons();

                rateStateButton.SetActive(false);
                CommonUtility.SetObjectActive(commonData.loaderButton, true);

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    CommonUtility.SetObjectActive(commonData.loaderButton, false);

                    InitializeButtons();
                    OnRatedAction();
                }, commonData.loaderButtonShowDuration);
            }
        }


        // Feedback popup logic below
        private void OpenInStoreButton_OnClick()
        {
            GameServices.Instance.ProposalService.RateUsProposal.RateRequset();

#if UNITY_EDITOR
            Hide();
#endif
        }


        private void SendFeedbackButton_OnClick()
        {
            rateUsService.RateApplication();

            Hide();
        }


        private void InputField_OnValueChanged(string newText)
        {
            bool isEmptyText = string.IsNullOrEmpty(newText);
            sendFeedbackStateButton.SetActive(!isEmptyText);
            caret.SetActive(isEmptyText);

            bool wasTextChanged = false;

            if (FeedbackValidator.IsInputValid(newText))
            {
                previousText = newText;
            }
            else
            {
                newText = previousText;
                wasTextChanged = true;
            }

            if (wasTextChanged)
            {
                inputField.text = newText;
            }
        }


        private void InputField_Select(string value) =>
            MoveContainerToPosition(upContainerRoot.transform.position, MoveContainerDuration);


        private void InputField_Deselect(string value) =>
            MoveContainerToPosition(centerContainerRoot.transform.position, MoveContainerDuration);

        #endregion
    }
}
