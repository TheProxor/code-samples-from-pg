using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class CurrencyLevelObjectExtension : InspectorExtensionBase
    {
        #region Fields

        [SerializeField] private OptionsChoiceUI currencyOptionsChoice = default;
        [SerializeField] private FloatInputUi currencyCountInput = default;

        private EditorLevelCurrency currencyObject;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            currencyObject = levelObject as EditorLevelCurrency;

            if (currencyObject != null)
            {
                currencyOptionsChoice.Init("Type", new List<string>(Enum.GetNames(typeof(CurrencyType))), (int)currencyObject.CurrencyType);

                currencyCountInput.Init("Amount", currencyObject.CurrencyCount, 0.0f);
            }
        }


        protected override void SubscribeOnEvents()
        {
            currencyOptionsChoice.OnValueChanged += CurrencyOptionsChoice_OnValueChange;
            currencyCountInput.OnValueChange += CurrencyCountInput_OnValueChange;
        }


        protected override void UnsubscribeFromEvents()
        {
            currencyOptionsChoice.OnValueChanged -= CurrencyOptionsChoice_OnValueChange;
            currencyCountInput.OnValueChange -= CurrencyCountInput_OnValueChange;
        }

        #endregion



        #region Events handlers

        private void CurrencyOptionsChoice_OnValueChange(int value)
        {
            currencyObject.CurrencyType = (CurrencyType)value;
        }


        private void CurrencyCountInput_OnValueChange(float value)
        {
            currencyObject.CurrencyCount = value;
        }

        #endregion
    }
}
