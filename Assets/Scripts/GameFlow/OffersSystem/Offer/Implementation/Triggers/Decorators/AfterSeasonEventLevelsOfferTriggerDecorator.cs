using Drawmasters.Proposal;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.OffersSystem
{
    public class AfterSeasonEventLevelsOfferTriggerDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly SeasonEventProposeController seasonEventController;
        private readonly int minLevelForTrigger;

        #endregion



        #region Properties

        public override bool IsActive => 
            base.IsActive &&
            seasonEventController.CurrentLevel - 1 >= minLevelForTrigger;

        #endregion



        #region Class lifecycle

        public AfterSeasonEventLevelsOfferTriggerDecorator(SeasonEventProposeController _seasonEventController, int _minLevelForTrigger, BaseOfferTrigger _trigger) : base(_trigger)
        {
            seasonEventController = _seasonEventController;
            minLevelForTrigger = _minLevelForTrigger;
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            seasonEventController.OnRewardReceived += OnRewardReceived;
        }


        public override void Deinitialize()
        {
            seasonEventController.OnRewardReceived -= OnRewardReceived;

            base.Deinitialize();
        }

        
        
        
        #endregion



        #region Events handlers

        private void OnRewardReceived(SeasonEventRewardType type, int index)
        {
            if (IsActive)
            {
                InvokeActiveEvent();
            }
        }

        #endregion
    }
}
