using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;


namespace Drawmasters.Proposal
{
    public class HappyHoursSeasonEventProposeController : LiveOpsEventController
    {
        #region Fields

        private readonly HappyHoursSettingsSeasonEvent settingsSeasonEvent;

        #endregion



        #region Properties
        
        public override float PointsMultiplier =>
            settingsSeasonEvent.PlayerPointsMultiplier;

        
        protected override string PrefsMainKey =>
            PrefsKeys.Proposal.LiveOpsEvent.HappyHoursSeasonEventMainKey;

        #endregion



        #region Class lifecycle

        public HappyHoursSeasonEventProposeController(LiveOpsProposeController _liveOpsProposeController,
                                                      HappyHoursVisualSettings _visualSettings,
                                                      HappyHoursSettingsSeasonEvent _abSettings,
                                                      IAbTestService _abTestService,
                                                      ICommonStatisticsService _commonStatisticsService,
                                                      IPlayerStatisticService _playerStatisticService,
                                                      ITimeValidator _timeValidator) :
            base(_liveOpsProposeController, _visualSettings, _abSettings, _abTestService, _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            settingsSeasonEvent = _abSettings;
        }

        #endregion
    }
}
