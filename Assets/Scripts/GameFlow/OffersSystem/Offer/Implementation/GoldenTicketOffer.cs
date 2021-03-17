using System;
using Drawmasters.Analytic;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;


namespace Drawmasters.OffersSystem
{
    public class GoldenTicketOffer: BaseOffer
    {
        #region Fields

        private readonly SeasonEventProposeController seasonEventProposeController;
        private readonly HappyHoursSeasonEventProposeController happyHoursSeasonEventProposeController;
        protected readonly IAbTestService abTestService;
        
        #endregion
        
        
        
        #region Overrided properties

        public override string OfferType =>
            OfferKeys.GoldenTicket;
        
        
        protected override string OfferPrefsMainKey =>
            PrefsKeys.Offer.GoldenTicket.MainKey;
        
        
        public override string ShowCounterKey =>
            string.Concat(
                string.Concat(OfferPrefsMainKey, seasonEventProposeController.PetMainRewardType.ToString()), 
                PrefsKeys.Proposal.LiveOps.PostfixShowCounter);

        #endregion
        
        
        
        #region Ctor
        
        public GoldenTicketOffer(IAbTestService _abTestService,
                                SeasonEventProposeController _seasonEventProposeController,
                                HappyHoursSeasonEventProposeController _happyHoursSeasonEventProposeController,
                                ICommonStatisticsService _commonStatisticsService,
                                IPlayerStatisticService _playerStatisticService,
                                ITimeValidator _timeValidator) :
            base(_abTestService.CommonData.goldenTicketOfferSettings, _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            abTestService = _abTestService;
            seasonEventProposeController = _seasonEventProposeController;
            happyHoursSeasonEventProposeController = _happyHoursSeasonEventProposeController;
        }
        
        #endregion



        #region Methods

        protected override BaseOfferTrigger[] CreateTrigers(string triger)
        {
            BaseOfferTrigger trigger = new BaseOfferTrigger();
            
            trigger = new CooldownOfferTriggerDecorator(this, trigger);
            
            if (triger.Equals(OfferKeys.TrigerTypesId.AfterMinStepClaimed))
            {
                GoldenTicketOfferSettings settings = offerSettings as GoldenTicketOfferSettings;

                trigger = new TimeFinishSeasonEventOfferTriggerDecorator(
                    seasonEventProposeController, 
                    settings.minTimeForStart,
                    trigger);    

                trigger = new AfterSeasonEventLevelsOfferTriggerDecorator(
                    seasonEventProposeController, 
                    settings.minSeasonEventStepClaimed,
                    trigger);    
            }
            
            if (triger.Equals(OfferKeys.TrigerTypesId.AlongWithHappyHours))
            {
                trigger = new AlongWithHappyHoursOfferTriggerDecorator(happyHoursSeasonEventProposeController, trigger);    
            }
            
            trigger = new OnceOfferTriggerDecorator(this, trigger);
            
            trigger = new GoldenTicketNotActiveDecorator(seasonEventProposeController, trigger);

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
            
            seasonEventProposeController.ProposeSeasonPass(onProposeHidden);
        }

        #endregion
    }
}