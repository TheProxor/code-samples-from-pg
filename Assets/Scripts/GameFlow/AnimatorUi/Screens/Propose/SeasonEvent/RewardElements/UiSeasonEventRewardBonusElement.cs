using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public class UiSeasonEventRewardBonusElement : UiSeasonEventRewardElement
    {
        #region Properties

        public override SeasonEventRewardType SeasonEventRewardType =>
            SeasonEventRewardType.Bonus;


        protected override bool ShouldSetBackNativeSize =>
            false;


        // hotfix vladislav k 
        protected override Vector3 UnlockFxScale =>
            Vector3.one.SetX(1.44f).SetY(1.2f);

        #endregion



        #region Methods

        public override void SetState(State _state, bool isImmediately)
        {
            base.SetState(_state, isImmediately);

            bool isMainRewardClaimed = GameServices.Instance.ProposalService.SeasonEventProposeController.IsRewardClaimed(SeasonEventRewardType.Main, 0);
            CommonUtility.SetObjectActive(gameObject, isMainRewardClaimed);
        }

        protected override void MarkRewardReceived(bool isRewardedVideo = false)
        {
            base.MarkRewardReceived(isRewardedVideo);
            
            GameServices.Instance.ProposalService.SeasonEventProposeController.MarkBonusRewardReceived();
        }

        #endregion
    }
}
