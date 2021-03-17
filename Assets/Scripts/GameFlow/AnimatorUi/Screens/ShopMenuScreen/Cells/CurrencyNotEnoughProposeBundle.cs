using System;
using Drawmasters.Proposal;
using Modules.General.InAppPurchase;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class CurrencyNotEnoughProposeBundle : CurrencyBundleShopCell
    {
        #region Fields

        [SerializeField] private Image[] mainImages = default;

        [SerializeField] private GameObject oneCurrencyProposalsRoot = default;
        [SerializeField] private GameObject twoCurrencyProposalsRoot = default;

        #endregion



        #region Properties

        protected override string BuyFxKey => string.Empty;

        #endregion



        #region Methods

        /// <summary>
        /// This method select one of visual root according to proposed currency count. Only visual changes.
        /// </summary>
        /// <param name="isOneCurrencyProposal"></param>
        public void SetCurrencyCountProposed(bool isOneCurrencyProposal)
        {
            CommonUtility.SetObjectActive(oneCurrencyProposalsRoot, isOneCurrencyProposal);
            CommonUtility.SetObjectActive(twoCurrencyProposalsRoot, !isOneCurrencyProposal);
        }


        public override void Initialize(StoreItem _storeItem)
        {
            base.Initialize(_storeItem);

            Sprite sprite = IngameData.Settings.iapsRewardSettings.FindProposeUiSprite(_storeItem.Identifier);

            foreach (var mainImage in mainImages)
            {
                mainImage.sprite = sprite;
                mainImage.SetNativeSize();
            }

            CurrencyReward[] data = RewardData.currencyData;
            foreach (var iconData in currencyIconsData)
            {
                bool isCurrencyExists = Array.Exists(data, d => d.currencyType == iconData.type);
                CommonUtility.SetObjectActive(iconData.dataRoot, isCurrencyExists);
            }
        }

        #endregion
    }
}
