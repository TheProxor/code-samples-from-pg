using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public abstract class ModesRewardController : RewardController
    {
        #region Fields

        private readonly string levelsCounterPrefix;

        #endregion



        #region Class lifecycle

        protected ModesRewardController(string _levelsCounterPrefix,
                                     string _uaAllowKey,
                                     string _uaDeltaLevelsKey,
                                     IAbTestService _abTestService,
                                     ILevelEnvironment levelEnvironment) :
            base(_uaAllowKey,
                 _uaDeltaLevelsKey,
                 _abTestService,                 
                 levelEnvironment)
        {
            levelsCounterPrefix = _levelsCounterPrefix;
        }

        #endregion



        #region Properties

        public virtual bool IsAvailable(GameMode mode)
        {
            int completedLevels = GetCompletedLevelsCounter(mode);
            
            bool isEnoughLevels = true;
            isEnoughLevels &= completedLevels >= ActualCompletedDeltaCounterLevels;
            isEnoughLevels |= ActualCompletedDeltaCounterLevels == 0;
            
            bool result = true;
            result &= AllowToPropose;
            result &= isEnoughLevels;
            
            return result;
        }


        private int GetCompletedLevelsCounter(GameMode mode) =>
            CustomPlayerPrefs.GetInt(string.Concat(levelsCounterPrefix, mode), ActualCompletedDeltaCounterLevels + 1);


        private void SetCompletedLevelsCounter(GameMode mode, int value) =>
            CustomPlayerPrefs.SetInt(string.Concat(levelsCounterPrefix, mode), value);

        #endregion


        #region Protected methods
        
        protected void MarkProposed(GameMode mode) => 
            SetCompletedLevelsCounter(mode, 0);
        

        protected void IncrementCompletedLevels(GameMode mode)
        {
            int currentValue = GetCompletedLevelsCounter(mode);

            SetCompletedLevelsCounter(mode, currentValue + 1);
        }

        #endregion
    }
}
