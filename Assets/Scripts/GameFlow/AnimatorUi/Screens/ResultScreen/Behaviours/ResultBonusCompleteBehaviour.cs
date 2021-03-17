namespace Drawmasters.Ui
{
    public class ResultBonusCompleteBehaviour : ResultCommonCompleteBehaviour
    {
        #region Ctor

        public ResultBonusCompleteBehaviour(Data _data, ResultScreen screen) : 
            base(_data, screen)
        { }

        #endregion



        #region Overrided

        protected override CurrencyType BonusCurrencyType 
            => CurrencyType.Premium;

        #endregion
    }
}

