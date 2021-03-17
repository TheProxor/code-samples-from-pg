using System;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public class ResultRewardController : SingleModeRewardController
    {
        #region Fields

        public static readonly CurrencyType[] TypesForReward = { CurrencyType.Simple };

        #endregion



        #region Properties

        public override bool IsAvailable => base.IsAvailable && AbAllowToPropose;

        protected override bool AbAllowToPropose => abTestService.CommonData.isAllowedCurrencyBonus;

        protected override int AbLevelsDeltaCount => abTestService.CommonData.currencyBonusLevelsDelta;

        #endregion



        #region Class lifecycle

        public ResultRewardController(string _levelsCounterKey,
                                      string uaAllowKey,
                                      string _uaDeltaLevelsKey,
                                      IAbTestService abTestService,
                                      ILevelEnvironment levelEnvironment)
            : base(_levelsCounterKey,
                   uaAllowKey,
                   _uaDeltaLevelsKey,
                   abTestService,
                   levelEnvironment)
        { }

        #endregion



        #region Methods

        public int CurrencyMultiplier(int passedSeconds)
        {
            int result = abTestService.CommonData.rewardMultipler - passedSeconds;
            
            return result < abTestService.CommonData.minRewardMultiplier ? abTestService.CommonData.minRewardMultiplier : result;
        }

        
        public static bool IsBonusAvailableForCurrencyType(CurrencyType type) =>
            Array.Exists(TypesForReward, e => e == type);

        #endregion



        #region Protected

        protected override void OnLevelCompleted(GameMode mode)
        {
            if (AllowToPropose)
            {
                CompletedLevelsCounter++;
            }
        }

        #endregion
    }
}
