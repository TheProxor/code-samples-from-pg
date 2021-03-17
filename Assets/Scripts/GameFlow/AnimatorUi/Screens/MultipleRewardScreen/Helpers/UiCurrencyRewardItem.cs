using Drawmasters.Proposal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiCurrencyRewardItem : UiRewardItem
    {
        #region Fields

        [SerializeField] private Image currencyImage = default;
        [SerializeField] private TMP_Text currencyAmount = default;

        #endregion



        #region Overrided

        public override void InitializeUiRewardItem(RewardData _rewardData, int sortingOrder)
        {
            base.InitializeUiRewardItem(_rewardData, sortingOrder);
            
            ApplyVisual(_rewardData as CurrencyReward);
        }

        #endregion
        
        
        
        #region Private methods

        private void ApplyVisual(CurrencyReward data)
        {
            if (data == null)
            {
                return;
            }

            Sprite sprite = IngameData.Settings.league.leagueRewardSettings.FindCurrencyRewardSprite(data.currencyType);

            currencyImage.sprite = sprite;
            currencyImage.SetNativeSize();

            currencyAmount.text = "+" + data.value.ToString("F0");
        }
        
        #endregion
    }
}