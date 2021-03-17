using Drawmasters.Proposal;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.OffersSystem
{
    public class TimeFinishSeasonEventOfferTriggerDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly SeasonEventProposeController seasonEventController;
        private readonly int timeToFinishForTrigger;

        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive && 
            seasonEventController.TimeLeftLiveOps.TotalSeconds >= timeToFinishForTrigger; 
        

        #endregion



        #region Class lifecycle

        public TimeFinishSeasonEventOfferTriggerDecorator(SeasonEventProposeController _seasonEventController, int _timeToFinishForTrigger, BaseOfferTrigger _trigger) : base(_trigger)
        {
            seasonEventController = _seasonEventController;
            timeToFinishForTrigger = _timeToFinishForTrigger;
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
