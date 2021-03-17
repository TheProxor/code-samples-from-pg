using System;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.OffersSystem;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiGoldenTicketOffer : UiOffer
    {
        #region Fields

        private BaseOffer offer;
        private SeasonEventProposeController controller;

        #endregion



        #region Properties

        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            controller.IsActive &&
            !controller.IsSeasonPassActive;


        public override IProposalController IProposalController =>
            GameServices.Instance.ProposalService.GetOffer<GoldenTicketOffer>();

        #endregion



        #region Methods


        public override void Initialize()
        {
            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            offer = GameServices.Instance.ProposalService.GetOffer<GoldenTicketOffer>();

            base.Initialize();
        }


        protected override void OnClickOpenButton(bool isForcePropose)
        {
            offer.ForcePropose(isForcePropose ? OfferKeys.EntryPoint.Auto : OfferKeys.EntryPoint.Widget);
            SetFadeEnabled(false);
        }

        #endregion
    }
}
