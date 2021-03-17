using Modules.General.InAppPurchase;
using TMPro;
using UnityEngine;
using Modules.InAppPurchase;
using Drawmasters.OffersSystem;
using Drawmasters.ServiceUtil;
using System;


namespace Drawmasters.Ui
{
    public class SubscriptionOfferBundleShopCell : TransitionShopCell
    {
        #region Fields

        [SerializeField] private TMP_Text timerText = default;
        [SerializeField] private TMP_Text priceOldText = default;
        [SerializeField] private RectTransformExpand priceOldExpandLine = default;

        private BaseOffer offer;

        private StoreItem oldStoreItem;

        #endregion




        #region Properties

        protected override bool ShouldShowRoot => base.ShouldShowRoot &&
                                                  offer.IsActive;

        #endregion


        #region Overrided methods

        public override void Initialize(StoreItem _storeItem)
        {
            offer = GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>();
            oldStoreItem = IAPs.GetStoreItem(IAPs.Keys.Subscription.Weekly);

            base.Initialize(_storeItem);

            priceOldExpandLine.Initialize();

            RefreshOldPrice();
        }


        public override void Deinitialize()
        {
            priceOldExpandLine.Deinitialize();

            base.Deinitialize();
        }

        #endregion



        #region Private methods

        protected override void RefreshVisual()
        {
            base.RefreshVisual();

            timerText.text = offer.TimeUi;
        }


        private void RefreshOldPrice()
        {
            if (oldStoreItem == null)
            {
                CustomDebug.Log("Season pass item is NULL");
                return;
            }

            if (!oldStoreItem.IsPriceActual)
            {
                StoreManager.Instance.RequestItemsData(oldStoreItem);
                StoreManager.Instance.ItemDataReceived += Instance_ItemDataReceived;
            }

            priceOldText.text = string.IsNullOrEmpty(oldStoreItem.LocalizedPrice) ?
                        $"${oldStoreItem.TierPrice:F2}" : oldStoreItem.LocalizedPrice;
        }
        
        #endregion
        
        
        
        #region Events handlers
        
        private void Instance_ItemDataReceived(StoreItem item)
        {
            if (item == oldStoreItem)
            {
                StoreManager.Instance.ItemDataReceived -= Instance_ItemDataReceived;
                RefreshOldPrice();
            }
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
