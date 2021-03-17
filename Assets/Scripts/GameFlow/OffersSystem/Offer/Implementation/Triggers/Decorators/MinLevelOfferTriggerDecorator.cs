using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.OffersSystem
{
    public class MinLevelOfferTriggerDecorator : BaseOfferTriggerDecorator
    {
        #region Fields

        private readonly ICommonStatisticsService statisticsService;
        private readonly int minLevelForTrigger;
        private readonly bool shouldCheckExactlyLevel;

        #endregion



        #region Properties

        public override bool IsActive =>
            base.IsActive && statisticsService.LevelsFinishedCount >= minLevelForTrigger;
            
        #endregion



        #region Class lifecycle

        public MinLevelOfferTriggerDecorator(ICommonStatisticsService _statisticsService, int _minLevelForTrigger, BaseOfferTrigger _trigger) : base(_trigger)
        {
            statisticsService = _statisticsService;
            minLevelForTrigger = _minLevelForTrigger;
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            statisticsService.OnLevelUp += OnLevelUp;
        }


        public override void Deinitialize()
        {
            statisticsService.OnLevelUp -= OnLevelUp;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void OnLevelUp()
        {
            if (IsActive)
            {
                InvokeActiveEvent();
            }
        }

        #endregion
    }
}
