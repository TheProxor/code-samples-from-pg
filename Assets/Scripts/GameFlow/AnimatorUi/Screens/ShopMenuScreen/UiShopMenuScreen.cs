using Sirenix.OdinInspector;
using System;
using Drawmasters.ServiceUtil;
using Drawmasters.OffersSystem;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;
using Modules.InAppPurchase;


namespace Drawmasters.Ui
{
    public class UiShopMenuScreen : UiIAPScreen
    {
        #region Helpers

        [Serializable]
        private class Cells
        {
            public BaseShopCell subscriptionOfferBundle = default;
            public BaseShopCell subscriptionBundle = default;

            public BaseShopCell seasonPassBundle = default;
            public BaseShopCell seasonPassOfferBundle = default;

            public BaseShopCell jackhammerBundle = default;

            public BaseShopCell mediumMoneyBundle = default;
            public BaseShopCell smallMoneyBundle = default;
            public BaseShopCell mediumBuildingKitBundle = default;
            public BaseShopCell smallBuildingKitBundle = default;

            public BaseShopCell noAds = default;
        }

        #endregion



        #region Fields

        [SerializeField] [Required] private Cells cellsReferences = default;

        #endregion



        #region Abstract implementation

        public override ScreenType ScreenType => ScreenType.ShopMenu;


        #endregion


        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null, Action<AnimatorView> onHideEndCallback = null, Action<AnimatorView> onShowBeginCallback = null, Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            InitializeCells();
            
            AbTestData abTestData = GameServices.Instance.AbTestService.CommonData;
            bool isNoAdsAvailable = abTestData.isNoAdsAvailable && 
                                    GameServices.Instance.CommonStatisticService.IsIapsAvailable &&
                                    !IAPs.IsStoreItemPurchased(IAPs.Keys.NonConsumable.NoAdsId);

            CommonUtility.SetObjectActive(cellsReferences.noAds.gameObject, isNoAdsAvailable);
        }

        
        protected override CellPair[] GetCells()
        {
            IProposalService proposalService = GameServices.Instance.ProposalService;

            return new CellPair[]
            {
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(proposalService.GetOffer<SubscriptionOffer>().InAppId),
                    cell = cellsReferences.subscriptionOfferBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Subscription.Weekly),
                    cell = cellsReferences.subscriptionBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.SeasonPass), 
                    cell = cellsReferences.seasonPassBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(proposalService.GetOffer<GoldenTicketOffer>().InAppId), 
                    cell = cellsReferences.seasonPassOfferBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.JackhammerSet), 
                    cell = cellsReferences.jackhammerBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.MediumPack), 
                    cell = cellsReferences.mediumMoneyBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.SmallPack), 
                    cell = cellsReferences.smallMoneyBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.MediumBuildingKit), 
                    cell = cellsReferences.mediumBuildingKitBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.BuildingKit), 
                    cell = cellsReferences.smallBuildingKitBundle
                },
                new CellPair
                {
                    storeItem = IAPs.GetStoreItem(IAPs.Keys.NonConsumable.NoAdsId), 
                    cell = cellsReferences.noAds
                }
            };
        }

        #endregion
    }
}