using UnityEngine;
using TMPro;
using Modules.UiKit;
using Modules.UiKit.FlowKit;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class DrawmastersSubscriptionRewardPopup : UiPopupReward
    {
        #region Fields

        [SerializeField] private TMP_Text currencyReward = default;

        [SerializeField] private TMP_Text gemsReward = default;

        #endregion


        #region Overrided

        public override void Initialize(IScreenSettings screenSettings, IScreenStateEvents stateEvents = null)
        {
            base.Initialize(screenSettings, stateEvents);

            if (screenSettings is IDrawmastersSubscriptionRewardSettings drawmastersSubscriptionSettings)
            {
                SetCurrencyRewardText(drawmastersSubscriptionSettings.SimpleCurrencyText);
                SetGemsRewardText(drawmastersSubscriptionSettings.PremiumCurrencyText);
            }
        }

        #endregion



        #region Private methods

        private void SetCurrencyRewardText(string text)
        {
            if(currencyReward.TryGetComponent(out Localize loc))
            {
                loc.SetStringParams(text);
            }
            else
            {
                currencyReward.text = text;
            }
        }
            

        private void SetGemsRewardText(string text)
        {
            if (gemsReward.TryGetComponent(out Localize loc))
            {
                loc.SetStringParams(text);
            }
            else
            {
                gemsReward.text = text;
            }
        }

        #endregion
    }
}
