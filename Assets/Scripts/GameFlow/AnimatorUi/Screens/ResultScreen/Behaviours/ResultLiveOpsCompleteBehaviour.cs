using Drawmasters.ServiceUtil;

namespace Drawmasters.Ui
{
    public class ResultLiveOpsCompleteBehaviour : ResultCommonCompleteBehaviour
    {
        #region Ctor

        public ResultLiveOpsCompleteBehaviour(Data _data, ResultScreen screen) 
            : base(_data, screen) { }

        #endregion



        #region Overrided

        protected override bool WasEarnedBonusCurrency
            => GameServices.Instance.LevelEnvironment.Progress.CurrencyPerLevelEnd(BonusCurrencyType) > 0;

        protected override string MultipliedRewardLabel(CurrencyType type)
            => GameServices.Instance.LevelEnvironment.Progress.UiLiveOpsCurrencyPerLevelEnd(type, forceMultiplied : true);

        protected override string UsualRewardLabel(CurrencyType type)
            => GameServices.Instance.LevelEnvironment.Progress.UiLiveOpsCurrencyPerLevelEnd(type, false);

        #endregion
    }
}
