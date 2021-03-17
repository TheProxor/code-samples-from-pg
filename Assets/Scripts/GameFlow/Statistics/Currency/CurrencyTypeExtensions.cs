using System;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Statistics.Data
{
    public static class CurrencyTypeExtensions
    {
        public static int ToIntCurrency(float value) =>
            (int)value;
        

        public static bool IsAvailableForShow(this CurrencyType currencyType)
        {
            bool result = true;

            result &= currencyType.IsMonopolyAvailableForShow();
            result &= currencyType.IsMansionAvailableForShow();

            return result;
        }


        public static bool IsMonopolyAvailableForShow(this CurrencyType currencyType)
        {
            bool isMonopolyAvailable = GameServices.Instance.ProposalService.MonopolyProposeController.IsMechanicAvailable;
            return currencyType.IsMonopolyCurrency() ? isMonopolyAvailable : true;
        }


        public static bool IsMonopolyAvailableForShowOnLevel(this CurrencyType currencyType)
        {
            MonopolyProposeController proposeController = GameServices.Instance.ProposalService.MonopolyProposeController;
            bool isMonopolyAvailable = proposeController.IsMechanicAvailable &&
                                       (proposeController.IsEnoughLevelsFinished || proposeController.IsActive);
            return currencyType.IsMonopolyCurrency() ? isMonopolyAvailable : true;
        }


        public static bool IsMansionAvailableForShow(this CurrencyType currencyType)
        {
            bool isMansionAvailable = GameServices.Instance.ProposalService.MansionProposeController.IsMechanicAvailable;
            return currencyType.IsMansionCurrency() ? isMansionAvailable : true;
        }


        public static bool IsMansionCurrency(this CurrencyType currencyType) =>
            Array.Exists(PlayerCurrencyData.MansionCurrencyTypes, e => e == currencyType);


        public static bool IsMonopolyCurrency(this CurrencyType currencyType) =>
            Array.Exists(PlayerCurrencyData.MonopolyCurrencyTypes, e => e == currencyType);

        public static bool IsLeagueCurrency(this CurrencyType currencyType) =>
            Array.Exists(PlayerCurrencyData.LeagueCurrencyTypes, e => e == currencyType);


        public static float ToPremium(this CurrencyType currencyType, float value)
        {
            float result = default;

            switch (currencyType)
            {
                case CurrencyType.MansionHammers:
                    result = value * 6;
                    break;

                case CurrencyType.RollBones:
                    result = value * 10;
                    break;

                default:
                    CustomDebug.LogError($"Can't convert {currencyType} to premium. No currency rate data.");
                    return value;
            }

            return result;
        }


        public static string UiCurrencyName(this CurrencyType currencyType)
        {
            string result;

            switch (currencyType)
            {
                case CurrencyType.Simple:
                    result = "Coins";
                    break;

                case CurrencyType.Premium:
                    result = "Gems";
                    break;

                case CurrencyType.MansionHammers:
                    result = "Hammers";
                    break;

                case CurrencyType.RollBones:
                    result = "Roll Bones";
                    break;

                case CurrencyType.Skulls:
                    result = "Skulls";
                    break;

                default:
                    CustomDebug.LogError($"Can't convert {currencyType} to premium. No currency rate data.");
                    result = string.Empty;
                    break;
            }

            return result;
        }
    }
}
