namespace Drawmasters.Ui
{
    public class UiMonopolySmallPropose : UiMonopolyBasePropose
    {
        #region Properties

        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            controller.IsActive;

        #endregion
    }
}
