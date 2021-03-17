namespace Drawmasters.Ui
{
    public class ResultBossCompleteBehaviour : ResultCommonCompleteBehaviour
    {
        public ResultBossCompleteBehaviour(Data _data, ResultScreen screen) : base(_data, screen)
        {
        }

        protected override CurrencyType BonusCurrencyType =>
            CurrencyType.Premium;
    }
}

