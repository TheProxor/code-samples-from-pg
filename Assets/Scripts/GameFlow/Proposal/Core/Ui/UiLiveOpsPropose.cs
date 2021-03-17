using System;
using Spine.Unity;
using UnityEngine;
using Drawmasters.Proposal.Ui;
using Drawmasters.Utils;


namespace Drawmasters.Proposal
{
    public abstract class UiLiveOpsPropose : UiProposal
    {
        #region Fields

        public static event Action<UiLiveOpsPropose> OnShouldForceProposeLiveOpsEvent;

        [Header("UiLiveOpsPropose")]
        
        [SerializeField] private GameObject activeRoot = default;
        [SerializeField] private GameObject reloadRoot = default;

        [Header("Happy hours. Optional")]
        [SerializeField] private SkeletonGraphic timerSkeletonGraphic = default;
        [SerializeField] private UiLiveOpsEvent[] uiLiveOpsEvents = default;

        #endregion



        #region Properties

        public override IProposalController IProposalController =>
            LiveOpsProposeController;

        public abstract LiveOpsProposeController LiveOpsProposeController { get; }

        protected abstract LiveOpsEventController LiveOpsEventController { get; }

        protected override PressButtonUtility.Data PressButtonData =>
            IngameData.Settings.commonRewardSettings.liveOpsesButtonPressData;

        public override bool CanForceProposeLiveOpsEvent =>
                LiveOpsEventController != null &&
                LiveOpsEventController.CanForcePropose &&
                ShouldShowProposalRoot;

        protected virtual bool ShouldDestroyTutorialCanvasAfterEvent =>
            false;

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            if (LiveOpsEventController != null)
            {
                foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
                {
                    uiLiveOpsEvent.Initialize(LiveOpsEventController);

                    // hack to hide visual before propose routine callback
                    uiLiveOpsEvent.SetForceProposePlaceAllowed(false);
                }

                InvokeForceProposeLiveOpsEvent(); // TODO: maybe redundant. to Vladislav.k
                LiveOpsEventController.OnStarted += InvokeForceProposeLiveOpsEvent;
            }

            if (timerSkeletonGraphic != null)
            {
                timerSkeletonGraphic.Initialize(false);
            }
        }


        public override void Deinitialize()
        {
            if (LiveOpsEventController != null)
            {
                LiveOpsEventController.OnStarted -= InvokeForceProposeLiveOpsEvent;

                foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
                {
                    uiLiveOpsEvent.Deinitialize();
                }
            }

            base.Deinitialize();
        }


        public override void ForceProposeEventWithMenuShow()
        {
            base.ForceProposeEventWithMenuShow();

            MonoBehaviourLifecycle.StopPlayingCorotine(forceProposeRoutine);
            forceProposeRoutine = MonoBehaviourLifecycle.PlayCoroutine(ProposeRoutine(() =>
            {
                if (ShouldDestroyTutorialCanvasAfterEvent)
                {
                    DestroyTutorialCanvas();
                    SetFadeEnabled(false);
                }
                else
                {
                    ProposeEvents();
                }
            }));
            LiveOpsProposeController.MarkForceProposed();

            void ProposeEvents()
            {
                RefreshTimeLeft();

                foreach (var uiLiveOpsEvent in uiLiveOpsEvents)
                {
                    uiLiveOpsEvent.SetForceProposePlaceAllowed(true);
                    uiLiveOpsEvent.ForcePropose();
                }
            }
        }

        protected override void OnRefreshTimeLeft()
        {
            base.OnRefreshTimeLeft();

            if (timerSkeletonGraphic != null && LiveOpsEventController != null)
            {
                timerSkeletonGraphic.Skeleton.SetSkin(LiveOpsEventController.UiTimerSkeletonGraphicSkin);
            }
        }


        protected override void RefreshRootGameObject()
        {
            base.RefreshRootGameObject();

            CommonUtility.SetObjectActive(activeRoot, IProposalController.IsActive);
            CommonUtility.SetObjectActive(reloadRoot, !IProposalController.IsActive);
        }

        #endregion



        #region Events handlers

        protected void InvokeForceProposeLiveOpsEvent()
        {
            if (CanForceProposeLiveOpsEvent)
            {
                OnShouldForceProposeLiveOpsEvent?.Invoke(this);
            }
        }

        #endregion
    }
}
