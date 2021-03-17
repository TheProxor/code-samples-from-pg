using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Modules.Advertising;
using Modules.General.Abstraction;
using Drawmasters.Analytic;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui.Enums;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.Proposal;
using Modules.General;
using Sirenix.OdinInspector;
using I2.Loc;
using static Drawmasters.Proposal.LeagueRewardController;
using UnityEngine.Events;

namespace Drawmasters.Ui
{
    public class UiMainMenuScreen : AnimatorScreen
    {
        #region Nested Types

        private abstract class LeagueRewardProposeSequenceElement : IProposeSequenceElement
        {
            #region Fields

            protected UiMainMenuScreen uiMainMenuScreen;

            #endregion



            #region Properties

            public UnityEvent OnCompleteSequenceElement { get; }


            protected LeagueProposeController leagueProposeController =>
                GameServices.Instance.ProposalService.LeagueProposeController;

            #endregion



            #region Ctor

            public LeagueRewardProposeSequenceElement(UiMainMenuScreen _uiMainMenuScreen)
            {
                uiMainMenuScreen = _uiMainMenuScreen;

                OnCompleteSequenceElement = new UnityEvent();
            }

            #endregion



            #region IProposeSequenceElement

            public void StartSequenceElementExecution(ProposeSequence sequence) =>
                Propose();
            

            public void StopSequenceElementExecution() =>
                OnCompleteSequenceElement.RemoveAllListeners();

            #endregion



            #region Methods

            protected void ShowMultipleRewardScreen(List<RewardData> data, string rewardTextKey, Action onScreenWasHided = default) =>
                uiMainMenuScreen.ShowMultipleRewardScreen(data, rewardTextKey, onScreenWasHided);
             
            #endregion



            #region Abstract Methods

            protected abstract void Propose();

            #endregion

        }


        private class LeagueIntermediateRewardProposeSequenceElement : LeagueRewardProposeSequenceElement
        {
            public LeagueIntermediateRewardProposeSequenceElement(UiMainMenuScreen _uiMainMenuScreen) : base(_uiMainMenuScreen) { }


            protected override void Propose()
            {
                if (leagueProposeController.RewardClaimController.TryClaimIntermediateReward(out List<RewardData> intermediateReward))
                {
                    uiMainMenuScreen.isSomeRewardProposeScreenShowed = true;

                    ShowMultipleRewardScreen(intermediateReward, string.Empty, Propose);
                }
                else
                {
                    OnCompleteSequenceElement.Invoke();
                    OnCompleteSequenceElement.RemoveAllListeners();
                }
            }
        }


        private class LeagueFinishRewardProposeSequenceElement : LeagueRewardProposeSequenceElement
        {
            public LeagueFinishRewardProposeSequenceElement(UiMainMenuScreen _uiMainMenuScreen) : base(_uiMainMenuScreen) { }


            protected override void Propose()
            {
                if (leagueProposeController.RewardClaimController.TryClaimFinishReward(out List<RewardData> finishReward, out List<PositionRewardData> uiLeagueRewardData))
                {
                    if (leagueProposeController.IsCurrentLiveOpsTaskCompleted)
                    {
                        AnalyticHelper.SendLiveOpsCompleteEvent(leagueProposeController.LiveOpsAnalyticName, leagueProposeController.LiveOpsAnalyticEventId);
                    }

                    uiMainMenuScreen.isSomeRewardProposeScreenShowed = true;

                    UiScreenManager.Instance.ShowScreen(ScreenType.LeagueEnd,
                        onShowBegin: (showView) => (showView as UiLeagueEndScreen)?.FillLeaderBord(uiLeagueRewardData.ToArray()),
                        onHideBegin: (hideView) => uiMainMenuScreen.RefreshActualUiHudTop(0.5f),
                        onHided: v =>
                        {
                            leagueProposeController.ResetSkullsCountCollectOnLastLevel();
                            leagueProposeController.LeaderBoard.Reset();
                            if (finishReward != null && finishReward.Count > 0)
                            {
                                ShowMultipleRewardScreen(finishReward, leagueProposeController.VisualSettings.finalRewardTextKey, Propose);
                            }
                            else
                            {
                                leagueProposeController.UiMainMenuScreen_OnFinishLeagueClaim();
                                Propose();
                            }
                        });
                }
                else
                {
                    OnCompleteSequenceElement.Invoke();
                    OnCompleteSequenceElement.RemoveAllListeners();
                }
            }
        }


        private class NewLeagueProposeSequenceElement : LeagueRewardProposeSequenceElement
        {
            public NewLeagueProposeSequenceElement(UiMainMenuScreen _uiMainMenuScreen) : base(_uiMainMenuScreen) { }


            protected override void Propose()
            {
                if (leagueProposeController.LeagueReachController.TryClaimNewReachedLeague(out LeagueType newLeague))
                {
                    uiMainMenuScreen.isSomeRewardProposeScreenShowed = true;

                    leagueProposeController.MarkAsProposed();

                    uiMainMenuScreen.rewardProposeSequence.OnComplete.AddListener(() => uiMainMenuScreen.RefreshModeVisual());

                    AnimatorScreen leagueChangeScreen = UiScreenManager.Instance.ShowScreen(ScreenType.LeagueChange, onHided:
                        view =>
                        {
                            Propose();
                        });

                    if (leagueChangeScreen is UiLeagueChangeScreen changeScreen)
                    {
                        changeScreen.SetupController(leagueProposeController, newLeague);
                    }
                }
                else
                {
                    OnCompleteSequenceElement.Invoke();
                    OnCompleteSequenceElement.RemoveAllListeners();
                }
            }
        }

        #endregion



        #region Fields

        [SerializeField] private UiHudTopSelector uiHudTopSelector = default;
        [SerializeField] private Localize currentLevelText = default;
        [SerializeField] private UiModeProgress uiModeProgress = default;
        [SerializeField] [Required] private Image fadeImage = default;

        [Header("With scroll LiveOps state")]
        [SerializeField] private MainMenuWithScrollLiveOpsBehaviour.Data withScrollLiveOpsData = default;
        
        [Header("Without scroll LiveOps state")]
        [SerializeField] private MainMenuWithoutScrollLiveOpsBehaviour.Data withoutScrollLiveOpsData = default;
        
        [Header("Combined collection state")]
        [SerializeField] private MainMenuCombinedCollectionBehaviour.Data combinedCollectionData = default;
        
        private IPlayerStatisticService playerStatistics;
        private LeagueProposeController leagueProposeController;
        
        private readonly Dictionary<MainMenuScreenState, IMainMenuBehaviour> behaviours =
            new Dictionary<MainMenuScreenState, IMainMenuBehaviour>();

        private ProposeSequence rewardProposeSequence = new ProposeSequence();

        private bool isSomeRewardProposeScreenShowed;

        #endregion



        #region Properties

        public override ScreenType ScreenType => ScreenType.MainMenu;

        public Image FadeImage => fadeImage;

        public float ScaleFactor => mainCanvas.scaleFactor;
        
        private UiHudTopSelector UiHudTopSelector => uiHudTopSelector;

        private IMainMenuBehaviour CurrentBehaviour
        {
            get
            {
                MainMenuScreenState state = CurrentMainMenuScreenState;
                IMainMenuBehaviour currentBehaviour = behaviours[state];
                if (currentBehaviour == null)
                {
                    CustomDebug.Log($"No find MainMenuBehaviour for state {state}");
                }
                return currentBehaviour;
            }
        }

        private MainMenuScreenState CurrentMainMenuScreenState
        {
            get
            {
                MainMenuScreenState result = MainMenuScreenState.WithScrollLiveOps;
                
                KeyValuePair<MainMenuScreenState, IMainMenuBehaviour> behaviour = 
                    behaviours.First(x => x.Value.IsMechanicAvailable);
                
                if (behaviour.Value == null)
                {
                    CustomDebug.Log("No find Available MainMenuBehaviour");
                }
                else
                {
                    result = behaviour.Value.ScreenState;
                }

                return result;
            }
        }


        public bool IsLeagueProposeAvaible =>
            leagueProposeController.IsInternetAvailable && leagueProposeController.IsMechanicAvailable;

        #endregion



        #region IInitializable

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
            Action<AnimatorView> onHideEndCallback = null,
            Action<AnimatorView> onShowBegin = null,
            Action<AnimatorView> onHideBegin = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBegin, onHideBegin);

            CommonUtility.SetObjectActive(fadeImage.gameObject, false);

            rewardProposeSequence.Initialize();

            behaviours.Add(MainMenuScreenState.WithScrollLiveOps,
                new MainMenuWithScrollLiveOpsBehaviour(withScrollLiveOpsData, this));

            behaviours.Add(MainMenuScreenState.WithoutScrollLiveOps,
                new MainMenuWithoutScrollLiveOpsBehaviour(withoutScrollLiveOpsData, this));

            behaviours.Add(MainMenuScreenState.CombinedCollection,
                new MainMenuCombinedCollectionBehaviour(combinedCollectionData, this));
            
            uiHudTopSelector.ShowActualUiHudTop();
            uiHudTopSelector.ActualUiHudTop.InitializeCurrencyRefresh();
            uiHudTopSelector.ActualUiHudTop.SetupExcludedTypes(CurrencyType.MansionHammers, CurrencyType.RollBones);
            RefreshActualUiHudTop();

            playerStatistics = GameServices.Instance.PlayerStatisticService;

            playerStatistics.CurrencyData.OnAnyCurrencyCountChanged += RefreshActualUiHudTop;
            playerStatistics.PlayerData.OnShooterSkinSetted += RefreshModeVisual;
            playerStatistics.PlayerData.OnPetSkinSetted += RefreshModeVisual;

            leagueProposeController = GameServices.Instance.ProposalService.LeagueProposeController;

            UiProposal.OnShouldDeinitializeButtons += DeinitializeButtons;
            UiProposal.OnShouldInitializeButtons += InitializeButtons;

            GameMode lastMode = playerStatistics.PlayerData.LastPlayedMode;

            GameServices.Instance.PetsService.RestoreController.TryRestoreDefaultPet();

            RefreshLevelText();
            
            uiModeProgress.Refresh(lastMode);            

            isSomeRewardProposeScreenShowed = false;
        }


        public override void Deinitialize()
        {
            playerStatistics.CurrencyData.OnAnyCurrencyCountChanged -= RefreshActualUiHudTop;
            playerStatistics.PlayerData.OnShooterSkinSetted -= RefreshModeVisual;
            playerStatistics.PlayerData.OnPetSkinSetted -= RefreshModeVisual;

            UiProposal.OnShouldDeinitializeButtons -= DeinitializeButtons;
            UiProposal.OnShouldInitializeButtons -= InitializeButtons;

            uiHudTopSelector.ActualUiHudTop.DeinitializeCurrencyRefresh();

            uiModeProgress.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            foreach (var behaviour in behaviours.Values)
            {
                behaviour.Deinitialize();
            }

            rewardProposeSequence.Deinitialize();

            isSomeRewardProposeScreenShowed = false;

            base.Deinitialize();
        }


        public override void InitializeButtons() =>
            CurrentBehaviour?.InitializeButtons();
        

        public override void DeinitializeButtons() =>
            CurrentBehaviour?.DeinitializeButtons();


        public override void Show()
        {
            base.Show();

            CurrentBehaviour.IsAnyForceProposeActive = true;

            RefreshModeVisual();
            ProposeAllLiveOps();
        }

        #endregion



        #region Methods

        public void ShowUiHudTop() => uiHudTopSelector.ActualUiHudTop.Show();


        public void HideUiHudTop() => uiHudTopSelector.ActualUiHudTop.Hide();


        public void ShowSkinScreen(ScreenType skinScreenType)
        {
            DeinitializeButtons();

            Hide(hidView =>
            {
                UiScreenManager.Instance.ShowScreen(skinScreenType, view =>
                    AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, AdPlacementType.GalleryOpen));
            }, null);
        }

       
        private void RefreshModeVisual()
        {
            RefreshLevelText();

            IMainMenuBehaviour currentBehaviour = CurrentBehaviour;
            
            foreach (var b in behaviours.Values)
            {
                b.Disable();
            }

            currentBehaviour?.Enable();
            
            IngameCamera.Instance.MoveLocalOffSetY(CurrentBehaviour.CameraOffsetSettings.offsetY, 
                CurrentBehaviour.CameraOffsetSettings.animation.duration,
                CurrentBehaviour.CameraOffsetSettings.animation.curve);
        }


        private void ProposeAllLiveOps()
        {
            rewardProposeSequence.OnComplete.AddListener(() =>
            {
                CurrentBehaviour.IsAnyForceProposeActive = leagueProposeController.NeedAnyPropose;
            }); 

            foreach (var proposal in combinedCollectionData.uiProposals)
            {
                rewardProposeSequence.Append(proposal);
            }

            AppendLeagueRewardPropose();
            
            rewardProposeSequence.Play();
        }


        private void AppendLeagueRewardPropose()
        {
            if (!IsLeagueProposeAvaible)
            {
                return;
            }

            AppendIntermediateLeagueRewardPropose();
            AppendFinishLeagueRewardPropose();
            AppendNewLeaguePropose();
        }


        private void AppendIntermediateLeagueRewardPropose()
        {
            rewardProposeSequence.Append(new LeagueIntermediateRewardProposeSequenceElement(this));

            rewardProposeSequence.OnComplete.AddListener(OnRewardWasProposed);

            void OnRewardWasProposed()
            {
                leagueProposeController.InvokeProposeListener();
            }
        }

        private void AppendFinishLeagueRewardPropose() =>
            rewardProposeSequence.Append(new LeagueFinishRewardProposeSequenceElement(this));


        private void AppendNewLeaguePropose() =>
            rewardProposeSequence.Append(new NewLeagueProposeSequenceElement(this));
        


        private void ShowMultipleRewardScreen(List<RewardData> data, string rewardTextKey, Action onScreenWasHided = default)
        {
            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                //HACK
                AnimatorScreen rewardScreen =
                UiScreenManager.Instance.ShowScreen(ScreenType.MultipleReward,
                    onHideBegin: (hideView) => RefreshActualUiHudTop(0.5f),
                    onHided: view =>
                    {
                        Scheduler.Instance.CallMethodWithDelay(this, () =>
                        {
                            onScreenWasHided?.Invoke();

                        }, CommonUtility.OneFrameDelay);
                    });

                if (rewardScreen is MultipleRewardScreen multipleRewardScreen)
                {
                    playerStatistics.CurrencyData.OnAnyCurrencyCountChanged -= RefreshActualUiHudTop;
                    UiHudTopSelector.ActualUiHudTop.DeinitializeCurrencyRefresh();

                    multipleRewardScreen.SetRewardTextKey(rewardTextKey);
                    multipleRewardScreen.SetupReward(data.ToArray());

                    playerStatistics.CurrencyData.OnAnyCurrencyCountChanged += RefreshActualUiHudTop;
                    uiHudTopSelector.ActualUiHudTop.InitializeCurrencyRefresh();
                }
            }, CommonUtility.OneFrameDelay);
        }


        private void RefreshLevelText()
        {
            GameMode lastMode = playerStatistics.PlayerData.LastPlayedMode;
            currentLevelText.SetStringParams(lastMode.UiHeaderText());
        }

        #endregion



        #region Events handlers

        private void RefreshActualUiHudTop() =>
            RefreshActualUiHudTop(0.0f);


        private void RefreshActualUiHudTop(float duration) =>
            uiHudTopSelector.ActualUiHudTop.RefreshCurrencyVisual(duration);

        #endregion


        #if UNITY_EDITOR

        #region Editor methods

        [Sirenix.OdinInspector.Button]
        private void ShowRewardScreen(bool isChest = false)
        {
            AnimatorScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.MultipleReward,
                onHideBegin: (hideView) => RefreshActualUiHudTop(0.5f));

            if (screen is MultipleRewardScreen mul)
            {
                playerStatistics.CurrencyData.OnAnyCurrencyCountChanged -= RefreshActualUiHudTop;
                uiHudTopSelector.ActualUiHudTop.DeinitializeCurrencyRefresh();


                RewardData shooterSkinReward = new ShooterSkinReward() { skinType = ShooterSkinType.Phoenix };
                List<RewardData> rewardData = new List<RewardData>();
                    
                rewardData.Add(shooterSkinReward);
                
                ChestData chestData = IngameData.Settings.league.chestSettings.GetChestData(ChestType.Epic);
                 rewardData.AddRange(chestData.RandomChestRewards.ToArray());

                if (isChest)
                {
                    ChestReward chestReward = new ChestReward();
                    rewardData.Add(chestReward);
                }
                
                mul.SetupReward(rewardData.ToArray());

                playerStatistics.CurrencyData.OnAnyCurrencyCountChanged += RefreshActualUiHudTop;
                uiHudTopSelector.ActualUiHudTop.InitializeCurrencyRefresh();
            }
        }

        
        [Sirenix.OdinInspector.Button]
        private void ShowLeagueEndScreen()
        {
            UiScreenManager.Instance.ShowScreen(ScreenType.LeagueEnd, onHideBegin: (hideView) => RefreshActualUiHudTop(0.5f), onHided: v =>{ });
        }


        [Sirenix.OdinInspector.Button]
        private void SetupAllProposals()
        {
            withScrollLiveOpsData.uiProposals = withScrollLiveOpsData.rootObject.GetComponentsInChildren<UiProposal>(true);
            withoutScrollLiveOpsData.uiProposals = withoutScrollLiveOpsData.rootObject.GetComponentsInChildren<UiProposal>(true);
            combinedCollectionData.uiProposals = combinedCollectionData.rootObject.GetComponentsInChildren<UiProposal>(true);
        }

        #endregion

        #endif
    }
}