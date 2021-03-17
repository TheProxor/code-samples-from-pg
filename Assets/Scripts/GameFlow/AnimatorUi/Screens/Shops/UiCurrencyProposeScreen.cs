using System;
using System.Collections.Generic;
using Modules.General.InAppPurchase;
using Modules.InAppPurchase;
using UnityEngine;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiCurrencyProposeScreen : UiIAPScreen
    {
        #region Fields

        [SerializeField] private UiHudTopSelector uiHudTopSelector = default;

        [SerializeField] private Localize headerLoc = default;
        [SerializeField] private CurrencyNotEnoughProposeBundle[] bundles = default;

        private CurrencyType currencyType;

        #endregion 



        #region Overrided properties

        public override ScreenType ScreenType => ScreenType.CurrencyPropose;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            uiHudTopSelector.ShowActualUiHudTop();

            uiHudTopSelector.ActualUiHudTop.InitializeCurrencyRefresh();
            uiHudTopSelector.ActualUiHudTop.SetupExcludedTypes(CurrencyType.RollBones);
            uiHudTopSelector.ActualUiHudTop.RefreshCurrencyVisual();
        }


        public override void Deinitialize()
        {
            uiHudTopSelector.ActualUiHudTop.DeinitializeCurrencyRefresh();

            base.Deinitialize();
        }


        public void SetupCurrencyType(CurrencyType _currencyType)
        {
            currencyType = _currencyType;

            string key = IngameData.Settings.iapsRewardSettings.FindHeaderKey(currencyType);
            headerLoc.SetTerm(key);

            InitializeCells();

            IapsRewardSettings.RewardData[] rewardData = IngameData.Settings.iapsRewardSettings.GetBankProposeRewardData(currencyType);

            List<CurrencyType> proposedTypes = new List<CurrencyType>();

            for (int i = 0; i < bundles.Length && i < rewardData.Length; i++)
            {
                int count = rewardData[i].currencyData.Length;
                bool isOneCurrencyProposed = count == 1;
                bundles[i].SetCurrencyCountProposed(isOneCurrencyProposed);

                foreach (var r in bundles[i].RewardData.currencyData)
                {
                    proposedTypes.Add(r.currencyType);
                }
            }

            uiHudTop.RefreshCurrencyVisual(default);
            uiHudTopSelector.ActualUiHudTop.RefreshCurrencyVisual(default);
        }


        protected override CellPair[] GetCells()
        {
            IapsRewardSettings.RewardData[] rewardData = IngameData.Settings.iapsRewardSettings.GetBankProposeRewardData(currencyType);

            CellPair[] result = Array.Empty<CellPair>();

            if (rewardData.Length != bundles.Length)
            {
                CustomDebug.LogWarning($"Bundles count {bundles.Length} and reward data {rewardData.Length} not euqals. Force hide screen.");
                HideImmediately();
                return Array.Empty<CellPair>();
            }

            for (int i = 0; i < bundles.Length && i < rewardData.Length; i++)
            {
                CellPair cellPair = new CellPair { storeItem = StoreManager.Instance.GetStoreItem(rewardData[i].storeId), cell = bundles[i] };
                result = result.Add(cellPair);
            }

            return result;
        }

        #endregion
    }
}
