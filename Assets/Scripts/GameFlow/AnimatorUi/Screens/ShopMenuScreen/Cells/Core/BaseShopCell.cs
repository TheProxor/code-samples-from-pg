using System;
using Drawmasters.Effects;
using Modules.General;
using Modules.General.InAppPurchase;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Drawmasters.Utils;
using Modules.InAppPurchase;

namespace Drawmasters.Ui
{
    public abstract class BaseShopCell : MonoBehaviour, IShopMenuCell
    {
        #region Fields

        [SerializeField] [Required] private TMP_Text costText = default;
        [SerializeField] private IdleEffect idleEffect = default;
        [SerializeField] private bool shouldUpdateFxOrder = true; // temp crunch for legacy fx system

        protected StoreItem storeItem;
        private LoopedInvokeTimer visualRefreshTimer;

        #endregion



        #region Properties

        public string StoreItemIdentifier => storeItem.Identifier;

        public IapsRewardSettings.RewardData RewardData
        {
            get
            {
                IapsRewardSettings.RewardData result = default;

                if (storeItem != null)
                {
                    result = IngameData.Settings.iapsRewardSettings.GetRewardData(storeItem.Identifier);
                }

                return result;

            }
        }


        protected virtual string BuyFxKey =>
            EffectKeys.FxGUIShopBuyItem;


        protected virtual bool ShouldShowRoot
        {
            get
            {
                bool result = true;

                if (storeItem != null)
                {
                    if (storeItem.IsNonConsumable)
                    {
                        result = !StoreManager.Instance.IsStoreItemPurchased(storeItem);
                    }
                    else if (storeItem.IsSubscription)
                    {
                        result = !StoreManager.Instance.HasAnyActiveSubscription;
                    }
                }

                return result;
            }
        }

        #endregion



        #region IShopMenuCell

        public event Action<IShopMenuCell> OnBecomeUnavailable;
        public event Action<BaseShopCell> OnBought;

        public virtual void Initialize(StoreItem _storeItem)
        {
            storeItem = _storeItem;
            
            //TODO HACK
            if (visualRefreshTimer != null && visualRefreshTimer.IsTimerActive)
            {
                visualRefreshTimer.Stop();
            }

            visualRefreshTimer = new LoopedInvokeTimer(RefreshVisual);
            visualRefreshTimer.Start();
            
            RefreshVisual();

            RefreshPrice();

            idleEffect.CreateAndPlayEffect();
        }


        public virtual void Deinitialize()
        {
            visualRefreshTimer.Stop();

            idleEffect.StopEffect();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void UpdateFxSortingOrder(int order)
        {
            if (shouldUpdateFxOrder)
            {
                idleEffect.UpdateSortingOrder(order);
            }
        }

        #endregion



        #region Private methods

        private void RefreshPrice()
        {
            StoreManager.Instance.ItemDataReceived -= Instance_ItemDataReceived;

            string price = storeItem.IsPriceActual ?
                storeItem.LocalizedPrice : $"{storeItem.TierPrice}$";

            costText.text = price;

            if (!storeItem.IsPriceActual)
            {
                StoreManager.Instance.ItemDataReceived += Instance_ItemDataReceived;
            }
        }


        protected void InvokeBoughtEvent() =>
            OnBought?.Invoke(this);


        protected void InvokeBecomeUnavailableEvent() =>
            OnBecomeUnavailable?.Invoke(this);
        
        #endregion



        #region Events handlers

        private void Instance_ItemDataReceived(StoreItem receivedItem)
        {
            if (receivedItem == storeItem)
            {
                RefreshPrice();
            }
        }


        protected virtual void RefreshVisual() =>
            CommonUtility.SetObjectActive(gameObject, ShouldShowRoot);

        #endregion
    }
}
