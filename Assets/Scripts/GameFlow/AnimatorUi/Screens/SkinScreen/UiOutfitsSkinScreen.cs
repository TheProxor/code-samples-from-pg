using System;
using System.Collections.Generic;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Modules.UiKit;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiOutfitsSkinScreen : UiSkinScreen
    {
        #region Fields

        [Header("Shooter skin scroll data")]
        [SerializeField]
        private UiSkinScrollData shooterSkinScrollData = default;

        [Header("Weapon skin scroll data")]
        [SerializeField]
        private UiSkinScrollData weaponSkinScrollData = default;

        [Header("Pet skin scroll data")]
        [SerializeField]
        private UiSkinScrollData petSkinScrollData = default;

        [Header("Tab buttons")] 
        [SerializeField] private Button shooterTabButton = default;
        [SerializeField] private Button weapontTabButton = default;
        [SerializeField] private Button petTabButton = default;

        [SerializeField] private Animator tabButtonsAnimator = default;
        [SerializeField] private AnimationEventsListener animationEventsListener = default;

        [SerializeField] private UiSkinPropose skinProposalIndicator = default;
        [SerializeField] private UiSkinPropose weaponProposalIndicator = default;

        #endregion


        
        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.OutfitsSkinScreen;


        protected override SkinScreenState InitialScrollState =>
             SkinScreenState.Shooter;


        protected override Dictionary<SkinScreenState, UISkinScrollBehaviour> InitialBehaviours
        {
            get
            {
                var result = new Dictionary<SkinScreenState, UISkinScrollBehaviour>
                {
                    { SkinScreenState.Shooter, new ShooterSkinScroll(shooterSkinScrollData) },
                    { SkinScreenState.Weapon, new WeaponSkinScroll(weaponSkinScrollData) },
                    { SkinScreenState.Pet, new PetSkinScroll(petSkinScrollData) }
                };

                return result;
            }
        }

        #endregion



        #region Methods

        public override void Show()
        {
            base.Show();
            
            weaponProposalIndicator.Initialize();
            weaponProposalIndicator.SetupSettings(GameServices.Instance.ProposalService.VideoWeaponSkinProposal as IAlertable);
            
            skinProposalIndicator.Initialize();
            skinProposalIndicator.SetupSettings(GameServices.Instance.ProposalService.VideoShooterSkinProposal as IAlertable);
        }


        public override void Deinitialize()
        {
            weaponProposalIndicator.Deinitialize();
            skinProposalIndicator.Deinitialize();

            base.Deinitialize();            
        }
        

        public override void InitializeButtons()
        {
            base.InitializeButtons();

            shooterTabButton.onClick.AddListener(ShooterTabButton_OnClick);
            weapontTabButton.onClick.AddListener(WeaponTabButton_OnClick);
            petTabButton.onClick.AddListener(PetTabButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            shooterTabButton.onClick.RemoveListener(ShooterTabButton_OnClick);
            weapontTabButton.onClick.RemoveListener(WeaponTabButton_OnClick);
            petTabButton.onClick.RemoveListener(PetTabButton_OnClick);

            base.DeinitializeButtons();
        }


        private void ShowTab(SkinScreenState state)
        {
            string triger = GetShowTabTriggerKey(scrollState, state);
            animationEventsListener.AddListener(RefreshBehaviour);
            
            tabButtonsAnimator.SetTrigger(triger);
            scrollState = state;
            currentBehaviour.Clear();
        }

        #endregion


        #region Events handlers

        private void ShooterTabButton_OnClick() =>
            ShowTab(SkinScreenState.Shooter);
        

        private void WeaponTabButton_OnClick() =>
            ShowTab(SkinScreenState.Weapon);
        

        private void PetTabButton_OnClick() =>
            ShowTab(SkinScreenState.Pet);
        
        #endregion
    }
}
