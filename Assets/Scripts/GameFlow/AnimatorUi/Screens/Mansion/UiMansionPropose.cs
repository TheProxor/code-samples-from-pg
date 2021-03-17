using System;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Utils;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui.Mansion;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiMansionPropose : UiProposal
    {
        #region Fields

        [SerializeField] private Button unavailableButton = default;

        [SerializeField] private GameObject availableVisualButtonRoot = default;
        [SerializeField] private GameObject unavailableVisualButtonRoot = default;

        #endregion



        #region Properties

        public override IProposalController IProposalController =>
            GameServices.Instance.ProposalService.MansionProposeController;


        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            true;


        public override bool ShouldShowProposalRoot =>
            true;


        protected override PressButtonUtility.Data PressButtonData =>
            IngameData.Settings.commonRewardSettings.liveOpsesButtonPressData;


        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            CommonUtility.SetObjectActive(availableVisualButtonRoot, IProposalController.IsMechanicAvailable);
            CommonUtility.SetObjectActive(unavailableVisualButtonRoot, !IProposalController.IsMechanicAvailable);
        }

        #endregion



        #region Events handlers

        protected override void OnClickOpenButton(bool isForcePropose)
        {
            AttemptAddTutorialCurrency();

            MansionProposeController mansionController = GameServices.Instance.ProposalService.MansionProposeController;

            Action<UiMansion> showCallback = isForcePropose ? (screen) => screen.MarkTutorialMenuEnter() : (Action<UiMansion>)default;
            mansionController.Propose(showCallback);


            void AttemptAddTutorialCurrency()
            {
                if (isForcePropose)
                {
                    PlayerCurrencyData playerCurrencyData = GameServices.Instance.PlayerStatisticService.CurrencyData;
                    if (playerCurrencyData.GetEarnedCurrency(CurrencyType.MansionHammers) <= 0.0f)
                    {
                        playerCurrencyData.AddCurrency(CurrencyType.MansionHammers, 1.0f);
                    }
                }
            }
        }


        protected override void OnClickNotEnoughLevelsFinished() =>
            UiScreenManager.Instance.ShowScreen(ScreenType.MansionMenu);

        #endregion
    }
}
