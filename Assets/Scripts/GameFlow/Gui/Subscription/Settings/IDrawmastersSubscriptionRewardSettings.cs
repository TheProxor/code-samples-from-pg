using Modules.UiKit;


namespace Drawmasters.Ui
{
    public interface IDrawmastersSubscriptionRewardSettings : IPopupRewardSettings
    {
        string SimpleCurrencyText { get; }

        string PremiumCurrencyText { get; }
    }
}
