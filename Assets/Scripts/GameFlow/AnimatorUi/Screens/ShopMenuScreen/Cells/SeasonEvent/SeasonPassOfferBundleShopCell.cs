using TMPro;
using UnityEngine;
using Drawmasters.Proposal;
using Drawmasters.OffersSystem;
using Drawmasters.ServiceUtil;
using Modules.General.InAppPurchase;
using Modules.InAppPurchase;
using System;


namespace Drawmasters.Ui
{
    public class SeasonPassOfferBundleShopCell : TransitionShopCell
    {
        #region Fields

        [SerializeField] private TMP_Text timerText = default;
        [SerializeField] private TMP_Text priceOldText = default;
        [SerializeField] private RectTransformExpand priceOldExpandLine = default;
        
        private StoreItem seasonPassItem;

        private SeasonEventProposeController controller;
        private BaseOffer offer;

        #endregion



        #region Overrided Properties

        protected override bool ShouldShowRoot => base.ShouldShowRoot &&
                                                  controller.IsMechanicAvailable &&
                                                  controller.IsActive &&
                                                  !controller.IsSeasonPassActive &&
                                                  offer.IsActive;

        #endregion



        #region Overrided methods

        public override void Initialize(StoreItem _storeItem)
        {
            seasonPassItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.SeasonPass);
            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            offer = GameServices.Instance.ProposalService.GetOffer<GoldenTicketOffer>();

            base.Initialize(_storeItem);

            priceOldExpandLine.Initialize();

            RefreshOldPrice();
        }


        public override void Deinitialize()
        {
            priceOldExpandLine.Deinitialize();

            base.Deinitialize();
        }


        protected override void RefreshVisual()
        {
            base.RefreshVisual();

            timerText.text = offer.TimeUi;
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

        #endregion



        #region Private methods

        private void RefreshOldPrice()
        {
            if (seasonPassItem == null)
            {
                CustomDebug.Log("Season pass item is NULL");
                return;
            }

            if (!seasonPassItem.IsPriceActual)
            {
                StoreManager.Instance.RequestItemsData(seasonPassItem);
                StoreManager.Instance.ItemDataReceived += Instance_ItemDataReceived;
            }

            priceOldText.text = string.IsNullOrEmpty(seasonPassItem.LocalizedPrice) ?
                        $"${seasonPassItem.TierPrice:F2}" : seasonPassItem.LocalizedPrice;
        }
        
        #endregion
        
        
        
        #region Events handlers
        
        private void Instance_ItemDataReceived(StoreItem item)
        {
            if (item == seasonPassItem)
            {
                StoreManager.Instance.ItemDataReceived -= Instance_ItemDataReceived;
                RefreshOldPrice();
            }
        }

        #endregion
    }
}
