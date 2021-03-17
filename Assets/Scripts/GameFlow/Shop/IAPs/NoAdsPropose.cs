using UnityEngine;
using UnityEngine.UI;
using Modules.General.InAppPurchase;
using Modules.InAppPurchase;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    public class NoAdsPropose : MonoBehaviour
    {
        #region Fields

        [SerializeField] private GameObject canBuyRoot = default;
        [SerializeField] private GameObject boughtRoot = default;
        [SerializeField] private GameObject lockedRoot = default;

        [SerializeField] private Button noAdsButton = default;

        private StoreItem noAdsItem;
        private StoreItem weeklySubscription;

        private IAPs IAPs;

        #endregion



        #region Properties

        private bool CanBuy => !IAPs.IsStoreItemPurchased(IAPs.Keys.NonConsumable.NoAdsId) &&
                               !IAPs.HasAnyActiveSubscription;

        #endregion



        #region Methods

        public void Initialize()
        {
            IAPs = GameServices.Instance.ShopService.IAPs;

            noAdsItem = IAPs.GetStoreItem(IAPs.Keys.NonConsumable.NoAdsId);
            weeklySubscription = IAPs.GetStoreItem(IAPs.Keys.Subscription.Weekly);

            noAdsButton.onClick.AddListener(NoAdsButton_OnClick);

            if (noAdsItem != null)
            {
                noAdsItem.PurchaseComplete += OnNoAdsItemReceived;
                noAdsItem.PurchaseRestored += OnNoAdsItemReceived;
            }

            if (weeklySubscription != null)
            {
                weeklySubscription.PurchaseComplete += OnSubscriptionItemReceived;
                weeklySubscription.PurchaseRestored += OnSubscriptionItemReceived;
            }

            if (noAdsItem != null && !noAdsItem.IsPriceActual)
            {
                StoreManager.Instance.RequestItemsData(noAdsItem);
            }

            RefreshUi();
        }


        public void Deinitialize()
        {
            noAdsButton.onClick.RemoveListener(NoAdsButton_OnClick);

            if (weeklySubscription != null)
            {
                weeklySubscription.PurchaseComplete -= OnSubscriptionItemReceived;
                weeklySubscription.PurchaseRestored -= OnSubscriptionItemReceived;
            }

            if (noAdsItem != null)
            {
                noAdsItem.PurchaseComplete -= OnNoAdsItemReceived;
                noAdsItem.PurchaseRestored -= OnNoAdsItemReceived;
            }
        }


        private void RefreshUi()
        {
            bool isAvailable = GameServices.Instance.AbTestService.CommonData.isNoAdsAvailable && 
                               GameServices.Instance.CommonStatisticService.IsIapsAvailable;
            
            CommonUtility.SetObjectActive(lockedRoot, !isAvailable);

            CommonUtility.SetObjectActive(canBuyRoot, isAvailable && CanBuy);

            if (boughtRoot != null)
            {
                CommonUtility.SetObjectActive(boughtRoot, isAvailable && !CanBuy);
            }
        }

        #endregion



        #region Events handlers

        private void NoAdsButton_OnClick()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                return;
            }

            if (!noAdsItem.IsPriceActual)
            {
                UiScreenManager.Instance.ShowPopup(OkPopupType.SomethingWentWrong);
                return;
            }

            UiScreenManager.Instance.ShowScreen(ScreenType.LoadingScreen);

            StoreManager.Instance.PurchaseItem(noAdsItem, (result) =>
            {
                UiScreenManager.Instance.HideScreen(ScreenType.LoadingScreen);
            });
        }


        private void OnNoAdsItemReceived(PurchaseItemResult result) =>
            RefreshUi();


        private void OnSubscriptionItemReceived(PurchaseItemResult result) =>
            RefreshUi();

        #endregion
    }
}
