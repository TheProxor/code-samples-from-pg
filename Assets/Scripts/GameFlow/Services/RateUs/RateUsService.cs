using System;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil.Interfaces;
using I2.Loc;

namespace Drawmasters.ServiceUtil
{
    public class RateUsService : IRateUsService
    {
        #region Nested types

        [Serializable]
        public class AbTestParams
        {
            public bool isCustomRateUsAvailable = default;
            public bool isIosNativeRateUsAvailalbe = default;
            public bool isRewardAvailable = default;
        }

        #endregion



        #region Fields

        private readonly IAbTestService abTestService;

        #endregion



        #region Class lifecycle

        public RateUsService(IAbTestService _abTestService)
        {
            abTestService = _abTestService;
        }

        #endregion



        #region IRateUsService

        public bool IsRated
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.PlayerInfo.IsRated, false);
            private set => CustomPlayerPrefs.SetBool(PrefsKeys.PlayerInfo.IsRated, value);
        }

        public string UiRewardText
        {
            get
            {
                RewardData rewardData = IngameData.Settings.commonRewardSettings.rateUsRewardData.RewardData;

                string result;

                if (rewardData is CurrencyReward currencyReward)
                {
                    result = $"+{currencyReward.value}";
                }
                else
                {
                    result = string.Empty;
                    CustomDebug.Log($"UiRewardText not implemented in {this} for rewardDataType = {rewardData.Type}");
                }

                return result;
            }
        }


        public bool IsAvailable
        {
            get
            {
                AbTestParams rateUsData = abTestService.CommonData.rateUsData;
                bool result = LLRatePopupManager.IsAvalaiblePopUp ?
                    rateUsData.isIosNativeRateUsAvailalbe : rateUsData.isCustomRateUsAvailable;

                return result;
            }
        }

        public bool IsRewardAvailable =>
            abTestService.CommonData.rateUsData.isRewardAvailable;


        public void RateApplication() =>
            IsRated = true;


        public void ApplyReward()
        {
            RewardData rewardData = IngameData.Settings.commonRewardSettings.rateUsRewardData.RewardData;
            rewardData.Open();
            rewardData.Apply();
        }


        public string GetDescriptionText(bool isRated)
        {
            // TODO: move into rate us settings
            string localizeKey;

            if (IsRewardAvailable)
            {
                localizeKey = isRated ? "loc_rateus_description_second" : "loc_rateus_description_first";
            }
            else
            {
                localizeKey = "loc_rateus_description_no_reward";
            }

            string result = LocalizationManager.GetTranslation(localizeKey);
            return result;
        }

        #endregion
    }
}

