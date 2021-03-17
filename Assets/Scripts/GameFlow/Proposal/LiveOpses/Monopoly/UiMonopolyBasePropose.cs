using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    public class UiMonopolyBasePropose : UiLiveOpsPropose
    {
        #region Fields

        protected MonopolyProposeController controller;

        #endregion



        #region Properties

        public override LiveOpsProposeController LiveOpsProposeController =>
            GameServices.Instance.ProposalService.MonopolyProposeController;

        
        protected override LiveOpsEventController LiveOpsEventController =>
            default;

        
        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            false;
        
        #endregion



        #region Methods

        public override void Initialize()
        {
            controller = GameServices.Instance.ProposalService.MonopolyProposeController;

            base.Initialize();
        }


        protected override void OnClickOpenButton(bool isForcePropose)
        {
            controller.Propose();
            controller.MarkForceProposed();
        }
        
        #endregion
    }
}
