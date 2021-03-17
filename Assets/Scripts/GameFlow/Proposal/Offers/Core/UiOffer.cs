using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Modules.General;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public abstract class UiOffer : UiProposal
    {
        #region Fields

        [Header("UiOffer")]
        [SerializeField] private IdleEffect uiOfferIdleEffect = default;
        [SerializeField] private Animator uiOfferAnimator = default;

        private ICommonStatisticsService statisticsService;

        #endregion



        #region Overrided Properties

        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            IProposalController.IsActive &&
            statisticsService.IsIapsAvailable;

        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            true;

        protected override PressButtonUtility.Data PressButtonData =>
            IngameData.Settings.commonRewardSettings.offersButtonPressData;

        #endregion



        #region Overrided Methods

        public override void Initialize()
        {
            statisticsService = GameServices.Instance.CommonStatisticService;

            base.Initialize();

            uiOfferIdleEffect.CreateAndPlayEffect();
        }


        protected override void OnPreForcePropose()
        {
            base.OnPreForcePropose();

            uiOfferIdleEffect.StopEffect();
            uiOfferAnimator.SetTrigger(AnimationKeys.Screen.Show);

            Scheduler.Instance.CallMethodWithDelay(this, () => SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SKULL_ADD),
                                                         IngameData.Settings.commonRewardSettings.offerAppearSoundDelay);
        }

        protected override void OnPostForcePropose()
        {
            uiOfferIdleEffect.CreateAndPlayEffect();

            base.OnPostForcePropose();
        }


        protected override void OnRefreshTimeLeft()
        {
            base.OnRefreshTimeLeft();

            RefreshGameObjects();
        }

        #endregion



        #region Events handlers

        protected override void IProposalController_OnFinished()
        {
            uiOfferAnimator.SetTrigger(AnimationKeys.Screen.Hide);

            // hotfix.
            Scheduler.Instance.CallMethodWithDelay(this, RefreshGameObjects,
                IngameData.Settings.commonRewardSettings.offerRefreshRootsAfterHideAnimationDelay);
        }

        #endregion
    }
}
