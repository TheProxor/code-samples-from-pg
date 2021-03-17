using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public class RouletteRewardController : ProposalResultController
    {
        #region Properties

        protected override ScreenType ScreenType => ScreenType.Roulette;

        protected override string ShowsCountKey => PrefsKeys.Proposal.RouletteLevelsCounter;

        #endregion



        #region Ua/Ab Properties

        protected override bool AbAllowToPropose => abTestService.CommonData.isRouletteResultProposalEnabled;

        protected override int AbLevelsDeltaCount => abTestService.CommonData.finishedLevelsCountForRoulettePropose;

        protected override int AbMinLevelForShopResultProposeKey => abTestService.CommonData.minFinishedLevelsForRoulettePropose;

        protected override string UaShopResultMinLevelKey => PrefsKeys.AbTest.UaRouletteMinLevel;

        #endregion



        #region Class lifecycle

        public RouletteRewardController(RouletteRewardPackSettings _settings,
                                           string _levelsCounterKey,
                                           string uaAllowKey,
                                           string _uaDeltaLevelsKey,
                                           IAbTestService abTestService,
                                           ILevelEnvironment levelEnvironment) :
            base(_settings,
                _levelsCounterKey,
                uaAllowKey,
                _uaDeltaLevelsKey,
                abTestService,
                levelEnvironment)
        {
        }

        #endregion



        #region Protected methods

        protected override void OnLevelCompleted(GameMode mode)
        {
            if (IsMinLevelReached(mode))
            {
                IncrementCompletedLevels(mode);
            }
        }

        #endregion
    }
}
