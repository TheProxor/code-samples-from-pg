using System;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using UnityEngine;
using Drawmasters.Helpers;
using Drawmasters.Effects;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSeasonEventBasePropose : UiLiveOpsPropose
    {
        #region Fields

        private const string PlayRootBounceId = "PlayRootBounceId";

        [Header("Announcer")]
        [SerializeField] private UiProposeAnnouncer proposeAnnouncer = default;

        [SerializeField] private Transform proposeAnnouncerStartRoot = default;
        [SerializeField] private Transform proposeAnnouncerFinishRoot = default;

        [SerializeField] private Animator rootBounceAnimator = default;
        [SerializeField] private AnimationEventsListener rootBounceEventsListener = default;

        protected SeasonEventProposeController controller;

        #endregion

        

        #region Properties

        public override LiveOpsProposeController LiveOpsProposeController =>
            GameServices.Instance.ProposalService.SeasonEventProposeController;

        
        protected override LiveOpsEventController LiveOpsEventController =>
            GameServices.Instance.ProposalService.HappyHoursSeasonEventProposeController;

        
        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            controller.ShouldShowPreviewScreen;

        #endregion

        

        #region Methods

        public override void Initialize()
        {
            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            
            base.Initialize();

            proposeAnnouncer.OnAnimationFinished += ProposeAnnouncer_OnAnimationFinished;
            ShowAnnouncer();
        }


        public override void Deinitialize()
        {
            if (controller == null)
            {
                CustomDebug.Log("Trying deinitialize controller without initialization. Lifecycle error has been founded.");
                return;
            }

            proposeAnnouncer.OnAnimationFinished -= ProposeAnnouncer_OnAnimationFinished;

            base.Deinitialize();
        }


        private void ShowAnnouncer()
        {
            float previousCurrency = controller.PointsCountOnPreviousShow;
            float currentCurrency = GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.SeasonEventPoints);

            bool shouldShowAnnouncer = AllowWorkWithProposal &&
                                       controller.IsActive &&
                                       !Mathf.Approximately(previousCurrency, currentCurrency);

            if (shouldShowAnnouncer)
            {
                proposeAnnouncer.SetupFxKey(EffectKeys.FxGUIMenuKeysFly);

                float receivedCurrency = currentCurrency - previousCurrency;
                proposeAnnouncer.Show(proposeAnnouncerStartRoot,
                                      proposeAnnouncerFinishRoot,
                                      string.Concat("+", receivedCurrency.ToShortFormat()));

                controller.PointsCountOnPreviousShow = currentCurrency;
            }
        }
        

        protected override void OnClickOpenButton(bool isForcePropose)
        {
            bool shouldShowPreviewScreen = controller.ShouldShowPreviewScreen;

            if (shouldShowPreviewScreen)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.SeasonEventPreview,
                    onShowed: (view) => SetFadeEnabled(false));

                controller.ShouldShowPreviewScreen = false;
            }
            else
            {
                DeinitializeButtons();

                controller.Propose();
            }
        }

        #endregion



        #region Events handlers

        private void ProposeAnnouncer_OnAnimationFinished()
        {
            rootBounceEventsListener.AddListener(PlayRootBounceId, () =>
            {
                EffectHandler effectHandler = EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMapElementUnlock,
                                                                                    proposeAnnouncerFinishRoot.transform.position,
                                                                                    default,
                                                                                    transform);
                effectHandler.transform.localScale *= 10.0f;
            });

            rootBounceAnimator.SetTrigger(AnimationKeys.Common.Bounce);
        }

        #endregion
    }
}