using System;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;


namespace Drawmasters.Ui
{
    [Serializable]
    public class SpinRouletteUiPropose : UiProposal
    {
        #region Fields

        private SpinRouletteController spinRouletteController;

        #endregion



        #region Properties

        public override IProposalController IProposalController =>
            GameServices.Instance.ProposalService.SpinRouletteController;


        public override bool ShouldShowProposalRoot =>
            IProposalController.IsMechanicAvailable;


        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            true;


        protected override PressButtonUtility.Data PressButtonData =>
            IngameData.Settings.commonRewardSettings.liveOpsesButtonPressData;

        #endregion



        #region UiProposal

        public override void Initialize()
        {
            spinRouletteController = GameServices.Instance.ProposalService.SpinRouletteController;

            base.Initialize();

            spinRouletteController.OnRewardRefreshed += RefreshButtonInteraction;
            spinRouletteController.OnFreeSpinned += RefreshButtonInteraction;

            RefreshButtonInteraction();
        }


        public override void Deinitialize()
        {
            if (spinRouletteController != null)
            {
                spinRouletteController.OnRewardRefreshed -= RefreshButtonInteraction;
                spinRouletteController.OnFreeSpinned -= RefreshButtonInteraction;
            }

            base.Deinitialize();
        }


        protected override void OnClickOpenButton(bool isForcePropose)
        {
            spinRouletteController.Propose();
            SetFadeEnabled(false);
        }

        #endregion



        #region Methods

        private void RefreshButtonInteraction()
        {
            bool isButtonInteractable = !spinRouletteController.CanForcePropose &&
                                        spinRouletteController.IsEnoughLevelsFinished &&
                                        spinRouletteController.IsMechanicAvailable;
            openButton.interactable = isButtonInteractable;
        }

        #endregion
    }
}
