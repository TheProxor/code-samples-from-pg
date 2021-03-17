using System;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.OffersSystem;
using Drawmasters.Proposal.Interfaces;
using Modules.General.InAppPurchase;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSubscriptionWidgetOffer : UiOffer
    {
        #region Fields

        private IForceProposalOffer offer;

        #endregion



        #region Properties

        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            !StoreManager.Instance.HasAnyActiveSubscription;


        public override IProposalController IProposalController =>
            GameServices.Instance.ProposalService.GetOffer<SubscriptionWidgetOffer>();

        #endregion


        
        #region Methods

        public override void Initialize()
        {
            offer = GameServices.Instance.ProposalService.GetOffer<SubscriptionWidgetOffer>();

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
