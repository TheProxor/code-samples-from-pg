using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;


namespace Drawmasters.Proposal
{
    public class HappyHoursLeagueProposeController : LiveOpsEventController
    {
        #region Fields

        private readonly HappyHoursSettingsLeague settingsLeague;

        #endregion



        #region Properties

        public float BotsSkullsMultiplier =>
            settingsLeague.BotsSkullsMultiplier;

        
        public override float PointsMultiplier =>
            settingsLeague.PlayerSkullsMultiplier;

        
        protected override string PrefsMainKey =>
            PrefsKeys.Proposal.LiveOpsEvent.HappyHoursLeagueMainKey;

        #endregion



        #region Class lifecycle

        public HappyHoursLeagueProposeController(LiveOpsProposeController _liveOpsProposeController,
                                                 HappyHoursVisualSettings _visualSettings,
                                                 HappyHoursSettingsLeague _abSettings,
                                                 IAbTestService _abTestService,
                                                 ICommonStatisticsService _commonStatisticsService,
                                                 IPlayerStatisticService _playerStatisticService,
                                                 ITimeValidator _timeValidator) :
            base(_liveOpsProposeController, _visualSettings, _abSettings, _abTestService, _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            settingsLeague = _abSettings;
        }

        #endregion
    }
}
