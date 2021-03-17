using Drawmasters.OffersSystem;
using Modules.General.InAppPurchase;
using Drawmasters.ServiceUtil;
using System;


namespace Drawmasters.Ui
{
    public class SubscriptionBundleShopCell : TransitionShopCell
    {
        #region Fields

        private BaseOffer offer;

        #endregion



        #region Properties

        protected override bool ShouldShowRoot => base.ShouldShowRoot &&
                                                  !offer.IsActive;

        #endregion



        #region Overrided methods

        public override void Initialize(StoreItem _storeItem)
        {
            offer = GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>();

            base.Initialize(_storeItem);
        }


        protected override void OnShouldMakeTransitional(Action onPurchased)
        {
            offer.ForcePropose(OfferKeys.EntryPoint.Store, () =>
            {
                if (StoreManager.Instance.HasAnyActiveSubscription)
                {
                    onPurchased?.Invoke();
                }
            });
        }

        #endregion
    }
}
