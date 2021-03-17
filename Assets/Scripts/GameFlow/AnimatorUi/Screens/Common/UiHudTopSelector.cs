using System;
using UnityEngine;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    // Use only to set correct visual from main menu
    [Serializable]
    public class UiHudTopSelector
    {
        #region Fields

        [Header("No Iaps header")]
        [SerializeField] private UiHudTop uiHudTop = default;

        [Header("Common header")]
        [SerializeField] private UiHudTop uiHudTopCurrencyPropose = default;

        #endregion



        #region Properties

        public UiHudTop ActualUiHudTop
        {
            get
            {

                bool isIAPsAvailable = GameServices.Instance.CommonStatisticService.IsIapsAvailable;
                
                return isIAPsAvailable ? uiHudTopCurrencyPropose : uiHudTop;
            }
        }

        #endregion



        #region Methods

        public void ShowActualUiHudTop()
        {
            bool isIAPsAvailable = GameServices.Instance.CommonStatisticService.IsIapsAvailable;

            CommonUtility.SetObjectActive(uiHudTop.gameObject, !isIAPsAvailable);
            CommonUtility.SetObjectActive(uiHudTopCurrencyPropose.gameObject, isIAPsAvailable);
        }

        #endregion
    }
}
