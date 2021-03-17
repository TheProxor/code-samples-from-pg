using System;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui.Enums;
using Modules.General;
using Sirenix.OdinInspector;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class MainMenuCombinedCollectionBehaviour : MainMenuBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data : BaseData
        {
            [Required] public Button outfitsButton = default;
        }

        #endregion


        #region Fields

        private readonly Data data;

        #endregion


        #region Properties

        public override MainMenuScreenState ScreenState =>
            MainMenuScreenState.CombinedCollection;

        protected override IUaAbTestMechanic uaAbTestMechanic =>
            new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuCombinedCollection);

        #endregion



        #region Ctor

        public MainMenuCombinedCollectionBehaviour(Data _data, UiMainMenuScreen screen) : base(_data, screen)
        {
            data = _data;
            objects.Add(data.rootObject);
        }

        #endregion


        #region IDeinitializable

        public override void Deinitialize()
        {
            data.proposeIndicator.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }

        #endregion


        #region IResultBehaviour

        public override void Enable()
        {
            IProposalService proposalService = GameServices.Instance.ProposalService;

            IAlertable[] alertableArray = new[]
            {
                proposalService.VideoShooterSkinProposal as IAlertable,
                proposalService.VideoWeaponSkinProposal as IAlertable
            };
            
            data.proposeIndicator.SetupSettings(alertableArray);

            base.Enable();

            data.proposeIndicator.Initialize();
        }


        public override void Disable()
        {
            data.proposeIndicator.Deinitialize();

            base.Disable();
        }


        public override void InitializeButtons()
        {
            base.InitializeButtons();

            data.outfitsButton.onClick.AddListener(OutfitsButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            data.outfitsButton.onClick.RemoveListener(OutfitsButton_OnClick);

            base.DeinitializeButtons();
        }

        #endregion


        #region Events hanlers

        protected virtual void OutfitsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.OutfitsSkinScreen);

        #endregion
    }
}