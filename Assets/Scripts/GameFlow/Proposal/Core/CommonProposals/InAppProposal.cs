using Drawmasters.Proposal.Interfaces;
using Drawmasters.Ui;
using System;
using Modules.General.InAppPurchase;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public class InAppProposal : IProposable
    {
        #region Fields

        private readonly StoreItem storeItem;

        #endregion



        #region Ctor

        public InAppProposal(StoreItem _storeItem)
        {
            storeItem = _storeItem;
        }

        #endregion



        #region IProposable

        public virtual bool CanPropose => true;

        public virtual bool IsAvailable => true;


        public void Propose(Action<bool> onProposed)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                onProposed?.Invoke(false);
                return;
            }

            if (!storeItem.IsPriceActual)
            {
                UiScreenManager.Instance.ShowPopup(OkPopupType.SomethingWentWrong);
                onProposed?.Invoke(false);
                return;
            }

            AnimatorView loadingView = UiScreenManager.Instance.ShowScreen(ScreenType.LoadingScreen);

            StoreManager.Instance.PurchaseItem(storeItem, purchaseResult =>
            {
                loadingView.Hide();

                bool wasPurchased = purchaseResult.IsSucceed;
                
                onProposed?.Invoke(wasPurchased);
            });

        }

        #endregion
    }
}
