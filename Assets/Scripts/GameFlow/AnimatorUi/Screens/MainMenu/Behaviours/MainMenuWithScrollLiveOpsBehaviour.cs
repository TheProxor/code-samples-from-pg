using System;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui.Enums;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General;
using Sirenix.OdinInspector;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class MainMenuWithScrollLiveOpsBehaviour : MainMenuBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data : BaseData
        {
            [Required] public UiSwipesRectSwitchController modeButtonsSwitchController = default;

            [Required] public Button shooterSkinsButton = default;
            [Required] public Button weaponSkinsButton = default;
            [Required] public Button petsButton = default;
        }

        #endregion


        #region Fields

        private readonly Data data;

        #endregion


        #region Properties

        public override MainMenuScreenState ScreenState =>
            MainMenuScreenState.WithScrollLiveOps;

        protected override IUaAbTestMechanic uaAbTestMechanic =>
            new CommonMechanicAvailability(PrefsKeys.AbTest.UaMainMenuWithScrollLiveOps);

        #endregion


        #region Ctor

        public MainMenuWithScrollLiveOpsBehaviour(Data _data, UiMainMenuScreen screen) : base(_data, screen)
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
            // Wait before object become active cuz we have to start coroutine. PS: Perfect external swipe screen logic
            data.modeButtonsSwitchController.Initialize(OnLiveOpsSwiped);

            uiLiveOpsProposeMonitor.OnShouldAddLiveOpsForSwipeScreen += UiLiveOpsProposeMonitor_OnShouldAddLiveOpsForSwipeScreen;
            uiLiveOpsProposeMonitor.OnShouldSwipeToLiveOps += UiLiveOpsProposeMonitor_OnShouldSwipeToLiveOps;

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                IUiProposal liveOpsToSwitch = uiLiveOpsProposeMonitor.LastSwipedUiLiveOpsProposal;
                data.modeButtonsSwitchController.SwitchTo(liveOpsToSwitch, true);

                data.modeButtonsSwitchController.PlaySetupSpacingContentCoroutine(mainMenuScreen.ScaleFactor, 740f);
            }, CommonUtility.OneFrameDelay);

            IProposalService proposalService = GameServices.Instance.ProposalService;

            
            data.proposeIndicator.SetupSettings(proposalService.VideoShooterSkinProposal as IAlertable);
            data.uiWeaponSkinPropose.SetupSettings(proposalService.VideoWeaponSkinProposal as IAlertable);

            // Call base.Enable() here because of uiLiveOpsProposeMonitor preparing
            base.Enable();

            data.proposeIndicator.Initialize();
            data.uiWeaponSkinPropose.Initialize();
        }


        public override void Disable()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            
            data.proposeIndicator.Deinitialize();
            data.uiWeaponSkinPropose.Deinitialize();

            if (uiLiveOpsProposeMonitor != null)
            {
                uiLiveOpsProposeMonitor.OnShouldAddLiveOpsForSwipeScreen -= UiLiveOpsProposeMonitor_OnShouldAddLiveOpsForSwipeScreen;
                uiLiveOpsProposeMonitor.OnShouldSwipeToLiveOps -= UiLiveOpsProposeMonitor_OnShouldSwipeToLiveOps;
            }

            base.Disable();
        }


        public override void InitializeButtons()
        {
            base.InitializeButtons();

            data.shooterSkinsButton.onClick.AddListener(ShooterSkinsButton_OnClick);
            data.weaponSkinsButton.onClick.AddListener(WeaponSkinsButton_OnClick);
            data.petsButton.onClick.AddListener(PetsButton_OnClick);
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

        private void OnLiveOpsSwiped()
        {
            if (uiLiveOpsProposeMonitor != null)
            {
                var lastSwiperProposal = data.modeButtonsSwitchController.Current as UiLiveOpsPropose;
                uiLiveOpsProposeMonitor.SetupLastSwipedUiLiveOpsProposal(lastSwiperProposal);
            }
        }


        private void UiLiveOpsProposeMonitor_OnShouldAddLiveOpsForSwipeScreen(IUiProposal uiLiveOpsProposal)
        {
            data.modeButtonsSwitchController.AddScreen(uiLiveOpsProposal);
            data.modeButtonsSwitchController.PlaySetupSpacingContentCoroutine(mainMenuScreen.ScaleFactor, 740f);
        }

        private void UiLiveOpsProposeMonitor_OnShouldSwipeToLiveOps(IUiProposal uiLiveOpsProposal) =>
            data.modeButtonsSwitchController.SwitchTo(uiLiveOpsProposal);

        private void ShooterSkinsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.ShooterSkinScreen);


        private void WeaponSkinsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.WeaponSkinScreen);


        private void PetsButton_OnClick() =>
            mainMenuScreen.ShowSkinScreen(ScreenType.PetSkinScreen);

        #endregion
    }
}