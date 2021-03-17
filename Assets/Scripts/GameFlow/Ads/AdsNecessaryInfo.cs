using System;
using Drawmasters.ServiceUtil;
using Modules.General.Abstraction;
using Modules.General.InAppPurchase;
using Modules.InAppPurchase;


namespace Drawmasters.Advertising
{
    public class AdsNecessaryInfo : IAdvertisingNecessaryInfo
    {
        #region IAdvertisingNecessaryInfo
        
        #pragma warning disable 0067

        // non required events
        public event Action<int> OnPlayerLevelUpdate;
        public event Action OnPersonalDataDeletingDetect;
        public event Action OnPurchasesListUpdate; 

        #pragma warning restore 0067

        public bool IsSubscriptionActive =>
            SubscriptionManager.Instance.IsSubscriptionActive;


        public bool IsNoAdsActive =>
            IAPs.IsStoreItemPurchased(IAPs.Keys.NonConsumable.NoAdsId) ||
            (GameServices.Instance.ProposalService.SeasonEventProposeController.IsActive &&
            GameServices.Instance.ProposalService.SeasonEventProposeController.IsSeasonPassActive);

 
        public bool IsPersonalDataDeleted =>
            GameManager.Instance.IsDataDeleted;


        public int CurrentPlayerLevel
        {
            get
            {
                if (GameServices.IsInstanceExist)
                {
                    return GameServices.Instance.CommonStatisticService?.UniqueLevelsFinishedCount ?? default;
                }
                else
                {
                    return default;
                }
            }
        }
        
        #endregion
    }
}
