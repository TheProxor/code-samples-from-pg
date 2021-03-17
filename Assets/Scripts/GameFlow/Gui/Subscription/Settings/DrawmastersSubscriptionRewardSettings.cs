using System;


namespace Drawmasters.Ui
{
    public class DrawmastersSubscriptionRewardSettings : IDrawmastersSubscriptionRewardSettings
    {
        #region IPopupRewardSettings

        public string RewardDescription { get; }

        public int RewardCount { get; }
        
        public Action<int> ClaimCallback { get; }

        #endregion



        #region IDrawmastersSubscriptionRewardSettings

        public string SimpleCurrencyText { get; }

        public string PremiumCurrencyText { get; }

        #endregion



        #region Ctor

        public DrawmastersSubscriptionRewardSettings(string rewardDescription,
            Action onClaim,
            string simpleCurrencyText,
            string premiumCurrencyText)
        {
            RewardDescription = rewardDescription;
            ClaimCallback = i => onClaim?.Invoke();
            SimpleCurrencyText = simpleCurrencyText;
            PremiumCurrencyText = premiumCurrencyText;
        }

        #endregion
    }
}