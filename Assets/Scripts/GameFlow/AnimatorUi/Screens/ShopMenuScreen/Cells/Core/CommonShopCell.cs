using Modules.InAppPurchase;
using Drawmasters.Effects;
using Modules.General;
using Modules.General.InAppPurchase;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class CommonShopCell : BaseShopCell
    {
        #region Fields

        [SerializeField] private float becomeUnavailablle = default;
        [SerializeField] [Required] private Button buyButton = default;

        #endregion



        #region IShopMenuCell

        public override void Initialize(StoreItem _storeItem)
        {
            base.Initialize(_storeItem);

            buyButton.onClick.AddListener(BuyRequest);
        }

        public override void Deinitialize()
        {
            buyButton.onClick.RemoveListener(BuyRequest);

            base.Deinitialize();
        }

        #endregion



        #region Private methods

        private void BuyRequest()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                return;
            }

            if (!storeItem.IsPriceActual)
            {
                UiScreenManager.Instance.ShowPopup(OkPopupType.SomethingWentWrong);
                return;
            }

            UiScreenManager.Instance.ShowScreen(ScreenType.LoadingScreen, view =>
            {
                StoreManager.Instance.PurchaseItem(storeItem, purchaseResult =>
                {
                    if (purchaseResult.IsSucceed)
                    {
                        OnItemBought();
                    }
                    else
                    {
                        UiScreenManager.Instance.HideScreen(ScreenType.LoadingScreen);
                    }
                });
            });
        }

        #endregion



        #region Events handlers

        protected virtual void OnItemBought()
        {
            EffectManager.Instance.PlaySystemOnce(BuyFxKey, parent: transform, transformMode: TransformMode.Local);

            InvokeBoughtEvent();

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                UiScreenManager.Instance.HideScreen(ScreenType.LoadingScreen);

                RefreshVisual();

                if (!storeItem.IsConsumable)
                {
                    InvokeBecomeUnavailableEvent();
                }
            }, becomeUnavailablle);
        }

        #endregion
    }
}
