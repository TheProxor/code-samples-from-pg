using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public class ShopResultController : ProposalResultController, IShowsCount
    {
        #region Class lifecycle

        public ShopResultController(SingleRewardPackSettings _settings,
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

        protected override bool AbAllowToPropose => abTestService.CommonData.isShopResultProposalEnabled;

        protected override int AbLevelsDeltaCount => abTestService.CommonData.finishedLevelsCountForShopResultPropose;

        protected override ScreenType ScreenType => ScreenType.ShopResult;

        protected override string UaShopResultMinLevelKey => PrefsKeys.AbTest.UaShopResultMinLevel;

        protected override int AbMinLevelForShopResultProposeKey => abTestService.CommonData.minFinishedLevelsForShopResultPropose;

        protected override string ShowsCountKey => PrefsKeys.Proposal.ShopShowCounter;

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
