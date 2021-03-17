using Drawmasters;
using Modules.General.InAppPurchase;


namespace Modules.InAppPurchase
{
    public class IAPs : IInitializable
    {
        #region Nested types

        public static class Keys
        {
            public static class Subscription
            {
                public const string Weekly = "diamondweekly";
                public const string WeeklySale = "diamondweekly.sale";
            }


            public static class Consumable
            {
                public const string JackhammerSet = "set.jackhammer";

                public const string SmallPack = "small.pack";
                public const string MediumPack = "medium.pack";
                public const string BuildingKit = "small.building.kit";
                public const string MediumBuildingKit = "medium.building.kit";

                public const string SeasonPass = "season.pass";
                public const string GoldenTicket = "golden.ticket";
            }

            public static class NonConsumable
            {
                public const string NoAdsId = "noads";
            }

            public static string[] AllKeys =>
                new []
                {
                    Consumable.JackhammerSet,
                    
                    Consumable.SmallPack,
                    Consumable.MediumPack,
                    Consumable.BuildingKit,
                    Consumable.MediumBuildingKit,
                    
                    Consumable.SeasonPass,
                    Consumable.GoldenTicket,
                    
                    NonConsumable.NoAdsId
                };
        }

        #endregion



        #region Properties
        
        private RewardHandler RewardHandler { get; set; }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            var weeklySubscription = GetStoreItem(Keys.Subscription.Weekly);
            if (weeklySubscription != null)
            {
                weeklySubscription.PurchaseRestored += WeeklySubscription_PurchaseRestored;
            }

            //noads doesn't matter, we don't get any reward
            RewardHandler = new RewardHandler(GetStoreItem(Keys.Consumable.JackhammerSet));
            RewardHandler.Initialize();
        }

        #endregion



        #region Public methods

        public static StoreItem GetStoreItem(string key)
        {
            StoreItem result = null;
            
            #if !CHINA_BUILD
                result = StoreManager.Instance.GetStoreItem(key);
            #endif

            return result;
        }

        public static bool IsStoreItemPurchased(string key)
            => GetStoreItem(key).IsPurchased();

        public static bool HasAnyActiveSubscription
            => StoreManager.Instance.HasAnyActiveSubscription;

        #endregion



        #region Events handlers

        private void WeeklySubscription_PurchaseRestored(PurchaseItemResult result)
        {
            if (HasAnyActiveSubscription)
            {
                IngameData.Settings.subscriptionRewardSettings.OpenSubscriptionReward();
            }
        }

        #endregion
    }
}
