using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;
using Drawmasters.Ui;
using System;


namespace Drawmasters.Proposal
{
    public class CurrencyProposal : IProposable
    {
        #region Fields

        private readonly (float, CurrencyType) currencyForPurchase;

        private readonly IPlayerStatisticService playerStatisticService;

        #endregion



        #region Ctor

        public CurrencyProposal((float, CurrencyType) _currencyForPurchase)
        {
            currencyForPurchase = _currencyForPurchase;

            playerStatisticService = GameServices.Instance.PlayerStatisticService;
        }

        #endregion



        #region IProposable

        public virtual bool CanPropose => true;

        public virtual bool IsAvailable => true;


        public void Propose(Action<bool> onProposed)
        {
            PlayerCurrencyData playerCurrencyInfo = playerStatisticService.CurrencyData;

            bool wasPurchased = playerCurrencyInfo.TryRemoveCurrency(currencyForPurchase.Item2, 
                currencyForPurchase.Item1);

            if (!wasPurchased)
            {
                bool isIAPsAvailable = GameServices.Instance.CommonStatisticService.IsIapsAvailable;

                if (isIAPsAvailable)
                {
                    GameServices.Instance.ProposalService.CurrencyShopProposal.Propose(currencyForPurchase.Item2);
                }
                else
                {
                    DialogPopupSettings settings = IngameData.Settings.dialogPopupSettings;
                    bool isPopupExist = settings.TryGetNotEnoughPopupType(currencyForPurchase.Item2, 
                            out OkPopupType popupType);

                    if (isPopupExist)
                    {
                        UiScreenManager.Instance.ShowPopup(popupType);
                    }
                }
            }

            onProposed?.Invoke(wasPurchased);
        }

        #endregion
    }
}
