using Drawmasters.Proposal;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Modules.General.InAppPurchase;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public abstract class UiIAPScreen : RewardReceiveScreen
    {
        #region Helpers

        public class CellPair
        {
            public StoreItem storeItem = default;
            public BaseShopCell cell = default;
        }

        #endregion



        #region Fields

        [SerializeField] [Required] private Button backButton = default;

        private ItemsHolder itemsHolder;

        private CellPair[] cells;

        #endregion



        #region Methods

        protected abstract CellPair[] GetCells();

        protected void InitializeCells()
        {
            cells = GetCells();
            cells.ToList().RemoveAll(i => i.storeItem == null);

            if (cells == null || cells.Length == 0)
            {
                CustomDebug.LogError($"Cells is null. Not initialized");
                return;
            }

            itemsHolder = new ItemsHolder(cells);
            itemsHolder.Initialize();
            itemsHolder.UpdateFxOrder(SortingOrder);


            foreach (var c in cells)
            {
                c.cell.OnBought += Cell_OnBought;
            }
        }


        public override void Initialize(Action<AnimatorView> onShowEndCallback = null, Action<AnimatorView> onHideEndCallback = null, Action<AnimatorView> onShowBeginCallback = null, Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            ViewManager.OnViewInfosChanged += ViewManager_OnViewInfosChanged;
            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.SetupExcludedTypes(CurrencyType.RollBones);
            uiHudTop.RefreshCurrencyVisual(0.0f);
        }

        public override void Deinitialize()
        {
            if (cells != null)
            {
                foreach (var c in cells)
                {
                    c.cell.OnBought -= Cell_OnBought;
                }
            }

            ViewManager.OnViewInfosChanged -= ViewManager_OnViewInfosChanged;
            itemsHolder?.Deinitialize();

            uiHudTop.DeinitializeCurrencyRefresh();

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            backButton.onClick.AddListener(Hide);
        }


        public override void DeinitializeButtons()
        {
            backButton.onClick.RemoveListener(Hide);
        }

        private void ViewManager_OnViewInfosChanged() =>
            itemsHolder.UpdateFxOrder(SortingOrder);

        #endregion



        #region Events handlers

        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            Vector3 result = Vector3.zero;

            if (rewardData is CurrencyReward currencyReward)
            {
                bool isSimpleCurrency = currencyReward.currencyType == CurrencyType.Simple;

                Predicate<CellPair> predicate = e => (e.cell.RewardData != null) &&
                                                     (e.cell.RewardData.currencyData.Contains(d => d == rewardData));
                CellPair foundElement = Array.Find(cells, predicate);

                if (foundElement.cell is CurrencyBundleShopCell currencyBundleShopCell)
                {
                    result = currencyBundleShopCell.GetIconPosition(currencyReward);

                    // CustomDebug.Log(result);
                }
            }

            return result;
        }


        private void Cell_OnBought(BaseShopCell boughtCell)
        {
            // if (boughtCell.RewardData == null)
            // {
            //     return;
            // }

            // uiHudTop.DeinitializeCurrencyRefresh();

            

            AnimatorScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.CongratulationScreen);
            UiCongratulationScreen congratulationScreen = screen as UiCongratulationScreen;
            
            if (congratulationScreen != null)
            {
                List<RewardData> rewards = new List<RewardData>();
                if (boughtCell.RewardData != null)
                {
                    if (boughtCell.RewardData.currencyData != null &&
                        boughtCell.RewardData.currencyData.Length > 0)
                    {
                        rewards.AddRange(boughtCell.RewardData.currencyData);
                    }

                    if (boughtCell.RewardData.shooterSkinReward != null &&
                        boughtCell.RewardData.shooterSkinReward.skinType != ShooterSkinType.None)
                    {
                        rewards.Add(boughtCell.RewardData.shooterSkinReward);
                    }
                }

                congratulationScreen.Initialize(rewards.ToArray(), boughtCell.StoreItemIdentifier, () =>
                {
                    if (boughtCell.RewardData != null)
                    {
                        for (int i = 0; i < boughtCell.RewardData.currencyData.Length; i++)
                        {
                            Action callback = (i == boughtCell.RewardData.currencyData.Length - 1) ? (Action)ApplySkinsReward : default;
                            OnShouldApplyReward(boughtCell.RewardData.currencyData[i], callback);
                        }

                        void ApplySkinsReward()
                        {
                            if (boughtCell.RewardData.shooterSkinReward.skinType != ShooterSkinType.None)
                            {
                                OnShouldApplyReward(boughtCell.RewardData.shooterSkinReward, true);
                            }
                        }
                    }
                });
                
            }
        }

        #endregion
    }
}
