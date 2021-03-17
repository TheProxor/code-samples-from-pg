using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public class UiSeasonEventRewardSimpleElement : UiSeasonEventRewardElement
    {
        #region Properties

        public override SeasonEventRewardType SeasonEventRewardType =>
            SeasonEventRewardType.Simple;


        protected override bool ShouldSetBackNativeSize =>
            true;


        protected override Vector3 UnlockFxScale =>
            Vector3.one;

        #endregion



        #region Methods

        protected override void MarkRewardReceived(bool isRewardedVideo = false)
        {
            base.MarkRewardReceived(isRewardedVideo);
            
            GameServices.Instance.ProposalService.SeasonEventProposeController.MarkRewardReceived(SeasonEventRewardType, localIndex);
        }

        #endregion
    }
}
