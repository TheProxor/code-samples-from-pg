namespace Drawmasters.Ui
{
    public class UiHitmastersSmallPropose : UiHitmastersBasePropose
    {
        #region Properties
        
        public override bool ShouldShowProposalRoot =>
            base.ShouldShowProposalRoot &&
            controller.IsActive;
        
        #endregion
    }
}
