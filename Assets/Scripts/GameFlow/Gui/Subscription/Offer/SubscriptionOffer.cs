using System;
using Drawmasters.Analytic;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using Modules.InAppPurchase;


namespace Drawmasters.OffersSystem
{
    public class SubscriptionOffer : BaseOffer
    {
        #region Fields

        protected readonly IAbTestService abTestService;

        #endregion



        #region Properties

        private bool AllowStartForOldUsers { get; set; }

        #endregion



        #region Overrided properties

        public override bool IsActive =>
            base.IsActive &&
            abTestService.CommonData.isSubscriptionAvailable;


        protected override bool CanStartOffer =>
            base.CanStartOffer &&
            AllowStartForOldUsers &&
            abTestService.CommonData.isSubscriptionAvailable;


        protected override string OfferPrefsMainKey =>
            PrefsKeys.Offer.Subscription.MainKey;


        public override string OfferType =>
            OfferKeys.VipSubscription;

        #endregion



        #region Ctor

        public SubscriptionOffer(IAbTestService _abTestService,
                                 ICommonStatisticsService _commonStatisticsService,
                                 IPlayerStatisticService _playerStatisticService,
                                 ITimeValidator _timeValidator) :
            base(_abTestService.CommonData.subscriptionOfferSettings, _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            abTestService = _abTestService;
        }


        protected override BaseOfferTrigger[] CreateTrigers(string trigers)
        {
            SubscriptionOfferSettings settings = offerSettings as SubscriptionOfferSettings;

            BaseOfferTrigger trigger = new BaseOfferTrigger();
            trigger = new CooldownOfferTriggerDecorator(this, trigger);
            trigger = new MinLevelOfferTriggerDecorator(commonStatisticsService, settings.minLevelForShow, trigger);
            trigger = new OnceOfferTriggerDecorator(this, trigger);
            trigger = new SubscriptionNotActiveDecorator(trigger);

            if (!CustomPlayerPrefs.HasKey(PrefsKeys.Subscription.OldUsersLevelsFinishedCount))
            {
                CustomPlayerPrefs.SetInt(PrefsKeys.Subscription.OldUsersLevelsFinishedCount, commonStatisticsService.LevelsFinishedCount);
            }

            int OldUsersLevelsFinishedCount = CustomPlayerPrefs.GetInt(PrefsKeys.Subscription.OldUsersLevelsFinishedCount);
            AllowStartForOldUsers = OldUsersLevelsFinishedCount <= settings.minLevelForShow;

            return new BaseOfferTrigger[] {
                trigger
                };
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
