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
    public class MainMenuWithoutScrollLiveOpsBehaviour : MainMenuBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data : BaseData
        {
            [Required] public Button shooterSkinsButton = default;
            [Required] public Button weaponSkinsButton = default;
            [Required] public Button petsButton = default;
        }

        #endregion


        #region Fields

        private readonly Data data;

        #endregion


        #region Properties

        public override MainMenuScreenState ScreenState => MainMenuScreenState.WithoutScrollLiveOps;

        protected override IUaAbTestMechanic uaAbTestMechanic =>
            new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuWithoutScrollLiveOps);

        #endregion


        #region Ctor

        public MainMenuWithoutScrollLiveOpsBehaviour(Data _data, UiMainMenuScreen screen) : base(_data, screen)
        {
            data = _data;
            objects.Add(data.rootObject);
        }

        #endregion


        #region IDeinitializable

        public override void Deinitialize()
        {
            data.proposeIndicator.Deinitialize();
            data.uiWeaponSkinPropose.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            base.Deinitialize();
        }

        #endregion


        #region IResultBehaviour

        public override void Enable()
        {
            IProposalService proposalService = GameServices.Instance.ProposalService;

            data.proposeIndicator.SetupSettings(proposalService.VideoShooterSkinProposal as IAlertable);
            data.uiWeaponSkinPropose.SetupSettings(proposalService.VideoWeaponSkinProposal as IAlertable);

            base.Enable();

            data.proposeIndicator.Initialize();
            data.uiWeaponSkinPropose.Initialize();
        }


        public override void Disable()
        {
            data.proposeIndicator.Deinitialize();
            data.uiWeaponSkinPropose.Deinitialize();

            base.Disable();
        }


        public override void InitializeButtons()
        {
            data.shooterSkinsButton.onClick.AddListener(ShooterSkinsButton_OnClick);
            data.weaponSkinsButton.onClick.AddListener(WeaponSkinsButton_OnClick);
            data.petsButton.onClick.AddListener(PetsButton_OnClick);

            base.InitializeButtons();
        }

        public override void DeinitializeButtons()
        {
            data.shooterSkinsButton.onClick.RemoveListener(ShooterSkinsButton_OnClick);
            data.weaponSkinsButton.onClick.RemoveListener(WeaponSkinsButton_OnClick);
            data.petsButton.onClick.RemoveListener(PetsButton_OnClick);

            base.DeinitializeButtons();
        }

        #endregion


        #region Events hanlers

        private void ShooterSkinsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.ShooterSkinScreen);


        private void WeaponSkinsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.WeaponSkinScreen);


        private void PetsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.PetSkinScreen);

        #endregion
    }
}