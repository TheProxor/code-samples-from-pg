using Drawmasters;
using Drawmasters.Ui;
using System.Collections.Generic;
using Modules.General.InAppPurchase;


namespace Modules.InAppPurchase
{
    public class RewardHandler : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly List<StoreItem> nonConsumableItems;

        #endregion



        #region Properties

        private bool NeedClaimImmediately => !UiScreenManager.Instance.IsScreenActive(ScreenType.ShopMenu);

        #endregion




        #region IInitializable

        public void Initialize()
        {
            foreach (var i in nonConsumableItems)
            {
                i.PurchaseRestored += NonConsumableItem_PurchaseRestored;
                i.PurchaseComplete += NonConsumableItem_PurchaseComplete;
            }
        }

        #endregion



        #region IDeiniializable

        public void Deinitialize()
        {
            foreach (var i in nonConsumableItems)
            {
                i.PurchaseRestored -= NonConsumableItem_PurchaseRestored;
                i.PurchaseComplete -= NonConsumableItem_PurchaseComplete;
            }

            nonConsumableItems.Clear();
        }

        #endregion



        #region Ctor

        public RewardHandler(params StoreItem[] items)
        {
            nonConsumableItems = new List<StoreItem>();

            foreach (var i in items)
            {
                if (i == null)
                {
                    continue;
                }
                
                bool isNonConsumableIap = i.ItemType == StoreItemType.NonConsumable && !StoreManager.Instance.IsStoreItemPurchased(i);

                if (isNonConsumableIap)
                {
                    nonConsumableItems.Add(i);
                }
            }
        }

        #endregion




        #region Public methods

        public void ClaimReward(StoreItem storeItem)
        {
            IapsRewardSettings.RewardData rewardData = IngameData.Settings.iapsRewardSettings.GetRewardData(storeItem.Identifier);

            if (rewardData != null)
            {
                ClaimReward(rewardData);
            }
            else
            {
                CustomDebug.Log("Reward data is NULL. Store item : " + storeItem.Identifier);
            }
        }

        #endregion



        #region Private methods

        private void RemoveItem(StoreItem handledItem)
        {
            if (handledItem.ItemType == StoreItemType.NonConsumable)
            {
                if (nonConsumableItems.Contains(handledItem))
                {
                    handledItem.PurchaseComplete -= NonConsumableItem_PurchaseComplete;
                    handledItem.PurchaseRestored -= NonConsumableItem_PurchaseRestored;

                    nonConsumableItems.Remove(handledItem);
                }
            }
        }


        private void ClaimReward(IapsRewardSettings.RewardData rewardData)
        {
            foreach (var reward in rewardData.currencyData)
            {
                reward.Open();
            }

            if (rewardData.shooterSkinReward.skinType != ShooterSkinType.None)
            {
                rewardData.shooterSkinReward.Open();
            }
        }


        private void ClaimImmediately(StoreItem storeItem)
        {
            if (NeedClaimImmediately)
            {
                ClaimReward(storeItem);
            }
        }

        #endregion



        #region Events handlers

        private void NonConsumableItem_PurchaseRestored(PurchaseItemResult restoreItemResult)
        {
            ClaimImmediately(restoreItemResult.StoreItem);

            RemoveItem(restoreItemResult.StoreItem);
        }


        private void NonConsumableItem_PurchaseComplete(PurchaseItemResult purchaseItemResult)
        {
            ClaimImmediately(purchaseItemResult.StoreItem);

            RemoveItem(purchaseItemResult.StoreItem);
        }

        #endregion
    }
}
