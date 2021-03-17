using System;
using Drawmasters.Analytic;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using Modules.InAppPurchase;


namespace Drawmasters.OffersSystem
{
    public class SubscriptionWidgetOffer : BaseOffer
    {
        #region Fields

        private readonly IAbTestService abTestService;
        
        private bool? wasForceProposed;
        
        #endregion



        #region Overrided properties

        public override bool IsActive =>
            IsMechanicAvailable &&
            IsAllTriggersActive &&
            commonStatisticsService.IsIapsAvailable &&
            abTestService.CommonData.isSubscriptionAvailable &&
            abTestService.CommonData.isSubscriptionWidgetAvailable;


        protected override bool CanStartOffer =>
            base.CanStartOffer &&
            abTestService.CommonData.isSubscriptionAvailable &&
            abTestService.CommonData.isSubscriptionWidgetAvailable;


        protected override string OfferPrefsMainKey =>
            PrefsKeys.Offer.Subscription.MainKey;


        
        protected override bool WasForceProposed 
        {
            get
            {
                if (!wasForceProposed.HasValue)
                {
                    wasForceProposed = LastShowDate.Date == DateTime.Today.Date;    
                }

                return wasForceProposed.Value;
            }
            set
            {
                wasForceProposed = value;
                LastShowDate = DateTime.Today;
            } 
        }

        
        private DateTime LastShowDate
        {
            get => CustomPlayerPrefs.GetDateTime(LastShowDateKey);
            
            set => CustomPlayerPrefs.SetDateTime(LastShowDateKey, value);
        }

        
        private string LastShowDateKey =>
            string.Concat(OfferPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixLastShowDate);


        public override string OfferType =>
            OfferKeys.VipFullSubscription;

        #endregion



        #region Ctor

        public SubscriptionWidgetOffer(IAbTestService _abTestService,
                                 ICommonStatisticsService _commonStatisticsService,
                                 IPlayerStatisticService _playerStatisticService,
                                 ITimeValidator _timeValidator) :
            base(_abTestService.CommonData.subscriptionWidgetOfferSettings, _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            abTestService = _abTestService;
        }

        #endregion
        
        
        
        #region Methods
        
        protected override BaseOfferTrigger[] CreateTrigers(string trigers)
        {
            SubscriptionOfferSettings settings = offerSettings as SubscriptionOfferSettings;

            BaseOfferTrigger trigger = new BaseOfferTrigger();
            trigger = new SubscriptionWidgetActiveDecorator(abTestService, trigger);
            trigger = new SubscriptionNotActiveDecorator(trigger);
            trigger = new MinLevelOfferTriggerDecorator(commonStatisticsService, settings.minLevelForShow, trigger);
            
            return new BaseOfferTrigger[] {
                trigger
                };
        }
        

        public override void Initialize()
        {
            InitializeTriggers();
        }
        
        #endregion



        #region IForceProposalOffer

        public override void ForcePropose(string entryPoint, Action onProposeHidden = default)
        {
            EntryPoint = entryPoint;
            
            if (IsActive)
            {
                AnalyticHelper.SendOfferShow(OfferType, EntryPoint);
            }
            
            SubscriptionHandler handler = new SubscriptionHandler();
            handler.HandleFromMenuSubscrption(onProposeHidden);
        }

        #endregion
    }
}
