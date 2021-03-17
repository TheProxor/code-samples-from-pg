using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public abstract class SingleModeRewardController : RewardController
    {
        #region Fields

        private readonly string levelsCounterKey;

        #endregion



        #region Properties

        public virtual bool IsAvailable => 
            CompletedLevelsCounter > ActualCompletedDeltaCounterLevels || 
            ActualCompletedDeltaCounterLevels == 0;


        protected int CompletedLevelsCounter
        {
            get => CustomPlayerPrefs.GetInt(levelsCounterKey, 0);
            set => CustomPlayerPrefs.SetInt(levelsCounterKey, value);
        }

        #endregion



        #region Ctor

        protected SingleModeRewardController(string _levelsCounterKey,
                                          string uaAllowKey,
                                          string _uaDeltaLevelsKey,
                                          IAbTestService abTestService,
                                          ILevelEnvironment levelEnvironment) :
            base(uaAllowKey,
                _uaDeltaLevelsKey,
                abTestService,
                levelEnvironment)
        {
            levelsCounterKey = _levelsCounterKey;
        }

        #endregion
    }
}
