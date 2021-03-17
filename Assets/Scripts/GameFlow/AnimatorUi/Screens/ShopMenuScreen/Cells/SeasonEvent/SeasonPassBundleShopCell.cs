using Modules.General.InAppPurchase;
using TMPro;
using UnityEngine;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.OffersSystem;
using System;


namespace Drawmasters.Ui
{
    public class SeasonPassBundleShopCell : TransitionShopCell
    {
        #region Fields

        [SerializeField] private TMP_Text timerText = default;

        private SeasonEventProposeController controller;
        private BaseOffer offer;

        #endregion



        #region Overrided properties

        protected override bool ShouldShowRoot => base.ShouldShowRoot &&
                                                  controller.IsMechanicAvailable &&
                                                  controller.IsActive &&
                                                  !controller.IsSeasonPassActive &&
                                                  !offer.IsActive;

        #endregion



        #region Overrided methods

        public override void Initialize(StoreItem _storeItem)
        {
            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            offer = GameServices.Instance.ProposalService.GetOffer<GoldenTicketOffer>();

            base.Initialize(_storeItem);
        }


        protected override void OnShouldMakeTransitional(Action onPurchased)
        {
            offer.ForcePropose(OfferKeys.EntryPoint.Store, () =>
            {
                if (controller.IsSeasonPassActive)
                {
                    onPurchased?.Invoke();
                }
            });
        }


        protected override void RefreshVisual()
        {
            base.RefreshVisual();

            timerText.text = controller.TimeUi;
        }

        #endregion
    }
}
