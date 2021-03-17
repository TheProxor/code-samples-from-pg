using UnityEngine;
using Drawmasters.Ui;
using Drawmasters.Effects;
using Drawmasters.Advertising;
using Drawmasters.ServiceUtil;
using Modules.General.Abstraction;


namespace Drawmasters.Proposal
{
    public class UiSeasonEventRewardMainElement : UiSeasonEventRewardElement
    { 
        #region Fields

        [SerializeField] private RewardedVideoButton receiveRewardVideoButton = default;

        [SerializeField] private IdleEffect[] idleEffects = default;

        private AdModule adModule;

        #endregion



        #region Properties

        public override SeasonEventRewardType SeasonEventRewardType =>
             SeasonEventRewardType.Main;


        protected override bool ShouldSetBackNativeSize =>
            false;


        protected override Vector3 UnlockFxScale =>
            Vector3.one.SetX(1.44f).SetY(1.2f);

        #endregion



        #region Methods

        public override void Initialize(SeasonEventProposeController _controller)
        {
            base.Initialize(_controller);

            adModule = GameServices.Instance.ProposalService.SeasonEventProposeController.AdModule;
            
            receiveRewardVideoButton.Initialize(AdsVideoPlaceKeys.SeasonEventScreenReward, adModule);
            receiveRewardVideoButton.OnVideoShowEnded += AdsReceiveRewardButton_OnVideoShowEnded;
        }


        public override void Deinitialize()
        {
            receiveRewardVideoButton.Deinitialize();
            receiveRewardVideoButton.OnVideoShowEnded -= AdsReceiveRewardButton_OnVideoShowEnded;

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.StopEffect();
            }

            base.Deinitialize();
        }


        public override void SetState(State _state, bool isImmediately)
        {
            base.SetState(_state, isImmediately);

            bool shouldShowAdsButton = _state == State.CanClaimForAds;

            bool shouldPlayIdleFx = _state == State.CanClaimForAds ||
                                    _state == State.NotReached ||
                                    _state == State.ReadyToClaim ||
                                    _state == State.ReachedCanNotClaim ||
                                    _state == State.ReachedCanNotClaimForAds;

            if (shouldPlayIdleFx)
            {
                foreach (var idleEffect in idleEffects)
                {
                    if (!idleEffect.IsCreated)
                    {
                        idleEffect.CreateAndPlayEffect();
                    }
                }
            }
            else
            {
                foreach (var idleEffect in idleEffects)
                {
                    idleEffect.StopEffect();
                }
            }

            if (shouldShowAdsButton)
            {
                receiveRewardVideoButton.DeinitializeButtons();
                receiveRewardVideoButton.InitializeButtons();
            }
            else
            {
                receiveRewardVideoButton.DeinitializeButtons();
            }

            if (_state == State.Claimed)
            {
                CommonUtility.SetObjectActive(body.gameObject, false);
            }
        }

        protected override void MarkRewardReceived(bool isRewardedVideo = false)
        {
            base.MarkRewardReceived(isRewardedVideo);
            
            GameServices.Instance.ProposalService.SeasonEventProposeController.MarkMainRewardReceived();
        }

        #endregion



        #region Events handlers

        private void AdsReceiveRewardButton_OnVideoShowEnded(AdActionResultType type) =>
            OnAdsWasWatched(adModule, type);

        #endregion
    }
}
