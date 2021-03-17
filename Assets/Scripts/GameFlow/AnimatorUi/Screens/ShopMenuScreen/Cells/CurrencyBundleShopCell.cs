using System;
using Modules.InAppPurchase;
using Drawmasters.Proposal;
using Modules.General.InAppPurchase;
using TMPro;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class CurrencyBundleShopCell : CommonShopCell
    {
        #region Nested types

        [Serializable]
        protected class CurrencyIconsData
        {
            public GameObject dataRoot = default;
            public CurrencyType type = default;
            public TMP_Text amount = default;
            [UnityEngine.Serialization.FormerlySerializedAs("rooot")]
            public Transform root = default;
        }

        #endregion



        #region Fields

        [SerializeField] protected CurrencyIconsData[] currencyIconsData = default;
        
        #endregion



        #region Overrided methods

        public override void Initialize(StoreItem _storeItem)
        {
            base.Initialize(_storeItem);

            RefreshVisual(RewardData);
        }

        #endregion



        #region Private methods

        protected virtual void RefreshVisual(IapsRewardSettings.RewardData rewardData)
        {
            if (rewardData == null)
            {
                return;
            }

            foreach (var reward in rewardData.currencyData)
            {
                var foundData = Array.FindAll(currencyIconsData, e => e.type == reward.currencyType);

                if (foundData != null)
                {
                    foreach (var data in foundData)
                    {
                        data.amount.text = $"+{reward.UiRewardText}";
                    }
                }
            }
        }


        public Vector3 GetIconPosition(CurrencyReward rewardData)
        {
            var foundElement = Array.Find(currencyIconsData, e => e.type == rewardData.currencyType);
            return foundElement == null ? default : foundElement.root.position;
        }

        #endregion
    }
}
