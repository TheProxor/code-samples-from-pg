using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Ui
{
    [CreateAssetMenu(fileName = "DialogPopupSettings",
                     menuName = NamingUtility.MenuItems.GuiSettings + "DialogPopupSettings")]
    public partial class DialogPopupSettings : ScriptableObject
    {
        #region Fields

        private readonly static Dictionary<CurrencyType, OkPopupType> currencyToPopup =
            new Dictionary<CurrencyType, OkPopupType>()
            {
                { CurrencyType.Simple, OkPopupType.NotEnoughCurrency},
                { CurrencyType.Premium, OkPopupType.NotEnoughGems},
                { CurrencyType.MansionHammers, OkPopupType.NotEnoughHammers}
            };

        [SerializeField] private DialogSettingsContainer[] dialogSettings = default;

        #endregion



        #region Methods

        public DialogSettingsContainer GetSettings(OkPopupType type)
        {
            return dialogSettings.Find((item) => item.dialogPopupType == type);
        }


        public bool TryGetNotEnoughPopupType(CurrencyType currencyType, out OkPopupType popupType)
        {
            bool isPopupExist = currencyToPopup.TryGetValue(currencyType, out popupType);
            if (!isPopupExist)
            {
                CustomDebug.Log($"No  not enough currency popup implementation found for currency type = {currencyType}");
            }

            return isPopupExist;
        }

        #endregion
    }
}
