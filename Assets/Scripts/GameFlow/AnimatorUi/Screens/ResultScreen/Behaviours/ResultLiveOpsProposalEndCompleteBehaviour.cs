namespace Drawmasters.Ui
{
    public class ResultLiveOpsProposalEndCompleteBehaviour : ResultCommonCompleteBehaviour
    {
        #region Ctor

        public ResultLiveOpsProposalEndCompleteBehaviour(Data _data, ResultScreen screen) 
            : base(_data, screen) { }

        #endregion


        #region Overrided

        protected override bool WasEarnedBonusCurrency 
            => false;

        protected override bool IsProposalSkinAvailable
            => false;

        #endregion
    }
}

