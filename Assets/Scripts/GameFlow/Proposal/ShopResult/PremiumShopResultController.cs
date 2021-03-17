using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public class PremiumShopResultController : ProposalResultController
    {
        #region Class lifecycle

        public PremiumShopResultController(SingleRewardPackSettings _settings,
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



        #region Ua/Ab Properties

        protected override bool AbAllowToPropose => abTestService.CommonData.isPremiumShopResultProposalEnabled;

        protected override int AbLevelsDeltaCount => abTestService.CommonData.finishedLevelsCountForPremiumShopResultPropose;

        protected override ScreenType ScreenType => ScreenType.PremiumShopResult;

        protected override string UaShopResultMinLevelKey => PrefsKeys.AbTest.UaPremiumShopResultMinLevel;

        protected override int AbMinLevelForShopResultProposeKey => abTestService.CommonData.minFinishedLevelsForPremiumShopResultPropose;

        protected override string ShowsCountKey => PrefsKeys.Proposal.PremiumShopShowCounter;

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