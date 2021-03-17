using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Drawmasters.Effects;
using Drawmasters.Mansion;
using TMPro;
using Modules.General;
using Modules.Sound;

namespace Drawmasters.Proposal
{
    public abstract class UiProposal : MonoBehaviour, IUiProposal, IProposeSequenceElement
    {
        #region Fields

        public static event Action<object> OnShouldForceStart; // crunch for case if live ops duration about ±10 sec

        public static event Action<bool> OnShouldSetLockedProposals;

        public static event Action<object> OnTutorialProposeStarted;
        public static event Action<object> OnTutorialProposeFinished;

        public static event Action OnShouldInitializeButtons;
        public static event Action OnShouldDeinitializeButtons;

        public static event Action OnShouldForcePropose;
        public event Action<IUiProposal> OnShouldAddProposal;
        public event Action<IUiProposal> OnShouldSwipeToProposal;

        protected const int ProposeSortingOrder = 1000;
        protected const int NormalSortingOrder = 21;

        [Header("Common Data")]
        [SerializeField] private RectTransform rootRectTransform = default;
        [SerializeField] private RectTransform scalableSwipeRect = default;

        [SerializeField] protected Button openButton = default;
        [SerializeField] private TMP_Text timerText = default;

        [SerializeField] private Image fadeImage = default;

        [Header("Optional params")]
        [SerializeField] private GameObject alertImage = default;

        [Header("Tutorial")]
        [SerializeField] [Enum(typeof(EffectKeys))] private string openFxKey = default;
        [SerializeField] [Enum(typeof(AudioKeys.Ingame))] private string openSfxKey = default;
        
        [SerializeField] private Transform openFxRoot = default;

        protected Coroutine forceProposeRoutine;

        private Canvas tutorialCanvas;

        private LoopedInvokeTimer alertRefreshTimer;
        private LoopedInvokeTimer timeLeftRefreshTimer;
    
        #endregion



        #region Properties

        public RectTransform SwipeRect =>
            rootRectTransform;


        public RectTransform ScalableSwipeRect =>
            scalableSwipeRect;


        protected bool AllowWorkWithProposal =>
            IProposalController.IsMechanicAvailable &&
            (IProposalController.IsActive || IProposalController.IsEnoughLevelsFinished);


        public bool CanStartSwipeWithProposal =>
            AllowWorkWithProposal &&
            IProposalController.IsActive;


        public virtual bool ShouldShowProposalRoot =>
            AllowWorkWithProposal;

        public abstract IProposalController IProposalController { get; }

        protected abstract bool ShouldDestroyTutorialCanvasAfterClick { get; }

        protected abstract PressButtonUtility.Data PressButtonData { get; }

        public object Key =>
            IProposalController;

        public bool CanForcePropose =>
            IProposalController.CanForcePropose;

        // hotfix to Vladislav.k
        public virtual bool CanForceProposeLiveOpsEvent { get; }

        public UnityEvent OnCompleteSequenceElement { get; } = new UnityEvent();

        #endregion



        #region Methods

        public virtual void Initialize()
        {
            alertRefreshTimer = alertRefreshTimer ?? new LoopedInvokeTimer(RefreshAlert);
            alertRefreshTimer.Start();
            RefreshAlert();

            timeLeftRefreshTimer = timeLeftRefreshTimer ?? new LoopedInvokeTimer(RefreshTimeLeft);
            timeLeftRefreshTimer.Start();
            RefreshTimeLeft();

            IProposalController.OnStarted += AttemptForcePropose;
            IProposalController.OnFinished += IProposalController_OnFinished;

            IProposalController.AttemptStartProposal();

            if (AllowWorkWithProposal)
            {
                RefreshGameObjects();
                RefreshVisual();
            }

            RefreshRootGameObject();

            if (ShouldShowProposalRoot)
            {
                OnShouldAddProposal?.Invoke(this);
            }
        }


        public virtual void Deinitialize()
        {
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            DestroyTutorialCanvas();

            if (IProposalController != null)
            {
                IProposalController.OnStarted -= AttemptForcePropose;
                IProposalController.OnFinished -= IProposalController_OnFinished;
            }
            else
            {
                CustomDebug.Log($"Attempt to deinitialize {this} without initialization");
            }

            MonoBehaviourLifecycle.StopPlayingCorotine(forceProposeRoutine);

            alertRefreshTimer?.Stop();
            timeLeftRefreshTimer?.Stop();

            if (forceProposeRoutine != null)
            {
                OnTutorialProposeFinished?.Invoke(Key); 
            }
        }


        public void InitializeButtons()
        {
            openButton.onClick.AddListener(OpenButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            openButton.onClick.RemoveListener(OpenButton_OnClick);
        }


        public void SetButtonsEnabled(bool isEnabled) =>
            openButton.interactable = isEnabled;


        // hotfix to Vladislav.k
        public virtual void ForceProposeEventWithMenuShow() { }


        public void ForceProposeWithMenuShow()
        {
            if (CanForcePropose)
            {
                forceProposeRoutine = MonoBehaviourLifecycle.PlayCoroutine(ProposeRoutine(onPreButtonClicked: () =>
                {
                    if (ShouldDestroyTutorialCanvasAfterClick)
                    {
                        DestroyTutorialCanvas();
                    }
                }));

                IProposalController.MarkForceProposed();
            }
        }


        protected virtual void OnPreForcePropose() { }


        protected virtual void OnPostForcePropose() { }


        protected virtual void RefreshRootGameObject() =>
            CommonUtility.SetObjectActive(rootRectTransform.gameObject, ShouldShowProposalRoot);
        

        private void RefreshAlert()
        {
            if (alertImage != null)
            {
                CommonUtility.SetObjectActive(alertImage, IProposalController.ShouldShowAlert);
            }
        }

        protected virtual void OnShouldRefreshVisual() { }

        protected virtual void OnRefreshTimeLeft() { }


        protected void SetFadeEnabled(bool isEnabled) =>
            CommonUtility.SetObjectActive(fadeImage.gameObject, isEnabled);


        protected abstract void OnClickOpenButton(bool isForcePropose);


        protected void RefreshTimeLeft()
        {
            if (!AllowWorkWithProposal)
            {
                return;
            }

            if (timerText != null)
            {
                timerText.text = IProposalController.TimeUi;
            }

            OnRefreshTimeLeft();
        }


        protected void InvokeShouldSetLockedProposals(bool enabled) =>
            OnShouldSetLockedProposals?.Invoke(enabled);


        protected void ShouldInitializeButtons() =>
            OnShouldInitializeButtons?.Invoke();


        protected void ShouldDeinitializeButtons() =>
            OnShouldDeinitializeButtons?.Invoke();


        private void RefreshVisual()
        {
            if (!AllowWorkWithProposal)
            {
                return;
            }

            OnShouldRefreshVisual();
        }


        protected IEnumerator ProposeRoutine(Action onBeginInteractible = default, Action onPreButtonClicked = default)
        {
            EventSystemController.SetSystemEnabled(false, this);

            SetFadeEnabled(true);
            fadeImage.color = fadeImage.color.SetA(default);

            OnTutorialProposeStarted?.Invoke(Key);

            yield return new WaitForEndOfFrame();

            tutorialCanvas = openButton.gameObject.GetComponent<Canvas>();
            if (tutorialCanvas == null)
            {
                tutorialCanvas = openButton.gameObject.AddComponent<Canvas>();
            }

            tutorialCanvas.overrideSorting = true;
            tutorialCanvas.sortingLayerName = "UI";

            tutorialCanvas.sortingOrder = ProposeSortingOrder;
            OnPreForcePropose();

            OnShouldSwipeToProposal?.Invoke(this);

            MansionRewardPackSettings settings = IngameData.Settings.mansionRewardPackSettings;
            settings.fadeAnimation.Play(value => fadeImage.color = fadeImage.color.SetA(value), this);

            yield return new WaitForSeconds(PressButtonData.beginInteractibleDelay);
            onBeginInteractible?.Invoke();

            yield return new WaitForSeconds(PressButtonData.tapDelay);

            openButton.animator.SetTrigger(openButton.animationTriggers.pressedTrigger);

            EffectManager.Instance.PlaySystemOnce(openFxKey,
                parent: openFxRoot,
                transformMode: TransformMode.Local);

            SoundManager.Instance.PlayOneShot(openSfxKey);

            yield return new WaitForSeconds(PressButtonData.tapDuration);

            openButton.animator.SetTrigger(openButton.animationTriggers.highlightedTrigger);
            openButton.animator.SetTrigger(openButton.animationTriggers.normalTrigger);

            yield return new WaitForSeconds(PressButtonData.afterTapDelay);

            OnPostForcePropose();

            if (!IProposalController.IsActive)
            {
                OnShouldForceStart?.Invoke(IProposalController);
            }

            onPreButtonClicked?.Invoke();
            openButton.onClick?.Invoke();

            forceProposeRoutine = null;

            EventSystemController.SetSystemEnabled(true, this);
            OnTutorialProposeFinished?.Invoke(Key);
        }


        protected void DestroyTutorialCanvas()
        {
            if (tutorialCanvas != null)
            {
                Destroy(tutorialCanvas);
                tutorialCanvas = null;
            }
        }


        public virtual void StartSequenceElementExecution(ProposeSequence sequence) =>
            OnCompleteSequenceElement?.Invoke();


        public virtual void StopSequenceElementExecution() =>
            OnCompleteSequenceElement.RemoveAllListeners();

        #endregion



        #region Events handlers

        protected virtual void IProposalController_OnFinished() =>
            RefreshGameObjects();


        protected void AttemptForcePropose()
        {
            RefreshGameObjects();
            RefreshTimeLeft();
            RefreshVisual();

            OnShouldForcePropose?.Invoke();
        }
        

        protected void RefreshGameObjects()
        {
            RefreshRootGameObject();
            RefreshAlert();
        }


        private void OpenButton_OnClick()
        {
            if (IProposalController.IsActive)
            {
                OnClickOpenButton(forceProposeRoutine != null);

                IProposalController.MarkForceProposed();

                if (!ShouldDestroyTutorialCanvasAfterClick)
                {
                    ShouldDeinitializeButtons();
                }
            }
            else if (!IProposalController.IsEnoughLevelsFinished)
            {
                OnClickNotEnoughLevelsFinished();
            }
            else
            {
                CustomDebug.Log($"No options for {this} in {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }
        }


        protected virtual void OnClickNotEnoughLevelsFinished() =>
            CustomDebug.Log($"Proposal {this} should not be available. Not enough levels finished.");

        #endregion
    }
}

