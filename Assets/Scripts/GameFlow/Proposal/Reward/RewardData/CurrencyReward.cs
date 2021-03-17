using System;
using Drawmasters.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class CurrencyReward : RewardData, IForcemeterReward
    {
        #region Fields

        private const string UiPlusRewardFormat = "+{0}";

        public CurrencyType currencyType = CurrencyType.Simple;
        public float value = default;

        public CurrencyReward()
        {
        }

        #endregion



        #region Properties

        public override RewardType Type =>
            RewardType.Currency;


        public override bool IsAvailableForRewardPack =>
            currencyType.IsAvailableForShow();


        public string UiRewardText =>
            value.ToShortFormat();

        public string UiPlusRewardText =>
            string.Format(UiPlusRewardFormat, value.ToShortFormat());


        public override string RewardText =>
            string.Format(AdsVideoRewardKeys.CurrencyRewardFormat, currencyType, (int) value);

        #endregion



        #region Methods

        public override void Open() =>
            GameServices.Instance.PlayerStatisticService.CurrencyData.AddCurrency(currencyType, value);

        public override Sprite GetUiSprite() =>
            IngameData.Settings.commonRewardSettings.FindCurrencyRewardSprite(currencyType);

        public Sprite GetForcemeterRewardSprite() =>
            IngameData.Settings.forceMeterRewardPackSettings.FindCurrencyRewardSprite(currencyType);


        public override bool IsEqualsReward(RewardData anotherReward)
        {
            bool result = base.IsEqualsReward(anotherReward);

            if (anotherReward is CurrencyReward currencyReward)
            {
                result = currencyType == currencyReward.currencyType &&
                         value == currencyReward.value;
            }

            return result;
        }

        #endregion
    }
}
