using System;
using Drawmasters.Ui;


namespace Drawmasters.Proposal
{
    public class CurrencyShopProposal
    {
        #region Public methods

        public void Propose(CurrencyType currencyType, Action onProposed = default)
        {
            AnimatorScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.CurrencyPropose, 
                onHided: view =>onProposed?.Invoke());

            if (screen is UiCurrencyProposeScreen uiCurrency)
            {
                uiCurrency.SetupCurrencyType(currencyType);
            }
        }

        #endregion
    }
}
