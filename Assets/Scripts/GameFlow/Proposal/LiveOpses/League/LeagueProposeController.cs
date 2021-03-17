using System;
using System.Linq;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Drawmasters.Levels;
using Drawmasters.Proposal.Helpers;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using GameFlow.Proposal.League;
using GameFlow.Proposal.League.LeagueReach;
using Modules.Networking;
using UnityEngine.Events;


namespace Drawmasters.Proposal
{
    public class LeagueProposeController : LiveOpsProposeController
    {
        #region Fields

        private readonly UnityEvent OnLeaguePointsRecieved = new UnityEvent();

        #endregion

       

        #region Properties

        public LeagueSettings Settings { get; }
        
        public LeagueVisualSettings VisualSettings { get; }

        public LeagueLeaderBoard LeaderBoard { get; }

        public ILeagueRewardController RewardController { get; }
        
        public ILeagueRewardClaimController RewardClaimController { get; }
        
        public ILeagueIntermediateRewardController IntermediateRewardController { get; }

        public LeagueReachController LeagueReachController { get; }

        public float SkullsCountCollectOnLastLevel { get; private set; } // Why it's not in player league data. to Dmitry.S

        public int PreviousPlayerPosition { get; private set; }

        public bool IsSkullsCollectAvailable =>
            IsMechanicAvailable && !RewardClaimController.CanClaimReward;

        public bool IsInternetAvailable =>
            LeagueAvailableOffline || ReachabilityHandler.Instance.NetworkStatus != NetworkStatus.NotReachable;
        
        public bool LeagueAvailableOffline { get;}
        
        public RewardData MainRewardData
        {
            get
            {
                UiLeagueRewardData data = RewardController.GetLeagueRewardPreviewData(CurrentLiveOpsId, LeaderBoard.LeagueType, 0);
                RewardData[] allMainRewardData = data == null ? default : data.RewardData;

                RewardData result = Array.Find(allMainRewardData, e => e.Type == RewardType.PetSkin);
                result = result ?? Array.Find(allMainRewardData, e => e.Type == RewardType.Currency &&
                                                                     (e as CurrencyReward).currencyType != CurrencyType.Simple);
                result = result ?? allMainRewardData.FirstOrDefault();

                return result;
            }
        }

        public bool NeedAnyPropose { get; private set; }

        #endregion



        #region Overrided properties

        public override bool ShouldShowAlert =>
            base.ShouldShowAlert && LeaderBoard.IsChangePlayerSkullCount;
        
        public override string LiveOpsAnalyticName => 
            LiveOpsNames.Tournament.Name;

        public override string LiveOpsAnalyticEventId =>
            LiveOpsNames.Tournament.GetEventName(LeaderBoard.LeagueType);

        public override string LiveOpsAnalyticPosition => 
            Convert.ToString(LeaderBoard.CurrentPlayerPosition);
        
        protected override string LiveOpsPrefsMainKey =>
            PrefsKeys.Proposal.LeagueMainKey;
        
        protected override string LiveOpsIdKey =>
            PrefsKeys.Proposal.League.LeagueIdKey;

        protected override bool CanStartLiveOps =>
            base.CanStartLiveOps && 
            IsInternetAvailable &&
            !RewardClaimController.CanClaimReward;
        
        #endregion



        #region Class lifecycle

        public LeagueProposeController(LeagueSettings _settings,
                                            LeagueVisualSettings _visualSettings,
                                            IAbTestService _abTestService,
                                            ICommonStatisticsService _commonStatisticsService,
                                            IPlayerStatisticService _playerStatisticService,
                                            ITimeValidator _timeValidator) :
                                            base(_settings.LiveOpsProposeSettings(_abTestService.CommonData), _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            Settings = _settings;
            VisualSettings = _visualSettings;

            LeagueAvailableOffline = _abTestService.CommonData.leagueAvailableOffline;
            
            LeaderBoard = new LeagueLeaderBoard(this, playerStatisticService, _timeValidator);

            playerStatisticService.CurrencyData.OnCurrencyAdded += CurrencyData_OnCurrencyAdded;

            RewardController = new LeagueRewardController(this, LeaderBoard);
            IntermediateRewardController = new LeagueIntermediateRewardController(_playerStatisticService);
            RewardClaimController = new LeagueClaimRewardController(this, RewardController, IntermediateRewardController);
            LeagueReachController = new LeagueReachController(this);
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            RewardController.Initialize();

            base.Initialize();

            LeaderBoard.UpdateLeaderList();
        }


        public void AddSkulls(float rawSkulls)
        {
            HappyHoursLeagueProposeController happyHoursController = GameServices.Instance.ProposalService.HappyHoursLeagueProposeController;
            bool wasLiveOpsEventActivedBeforeLevel = happyHoursController.WasActiveBeforeLevelStart;
            
            float playerEarnedSkulls = wasLiveOpsEventActivedBeforeLevel && happyHoursController.PointsMultiplier > 0 ?
                rawSkulls / happyHoursController.PointsMultiplier : rawSkulls;

            int roundedPlayerEarnedSkulls = CurrencyTypeExtensions.ToIntCurrency(playerEarnedSkulls);

            if (IsSkullsCollectAvailable && roundedPlayerEarnedSkulls > 0)
            {
                if (IsActive)
                {
                    PreviousPlayerPosition = LeaderBoard.CurrentPlayerPosition;
                    SkullsCountCollectOnLastLevel = rawSkulls;

                    LeaderBoard.BotController.AddSkulls(wasLiveOpsEventActivedBeforeLevel ? happyHoursController.BotsSkullsMultiplier : 1.0f);
                    IntermediateRewardController.AddLeaguePoints(SkullsCountCollectOnLastLevel);
                    RewardClaimController.RecalculateFinishReward();
                }

                LeaderBoard.BotController.RecalculatePlayerFactor(roundedPlayerEarnedSkulls);
            }

            if (!IsInternetAvailable)
            {
                playerStatisticService.CurrencyData.TryRemoveCurrency(CurrencyType.Skulls, rawSkulls);
                ResetSkullsCountCollectOnLastLevel();
            }

            LeaderBoard.UpdateLeaderList();
        }
        

        public override void Propose()
        {
            UiScreenManager.Instance.HideScreen(ScreenType.LeaguePreview);
            UiScreenManager.Instance.HideScreen(ScreenType.LeagueLeaderBoard);

            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);
                LevelsManager.Instance.UnloadLevel();

                if (LeaderBoard.ShouldShowLeagueApplyScreen)
                {
                    UiScreenManager.Instance.ShowScreen(ScreenType.LeagueApply, onShowBegin: (showView) =>
                    {
                        LeaderBoard.ShouldShowLeagueApplyScreen = false;
                        if (showView is UiLeagueApplyScreen uiLeagueApplyScreen)
                        {
                            uiLeagueApplyScreen.SetupController(this);
                        }
                    }, isForceHideIfExist: true);
                }
                else
                {
                    ProposeLeaderBoard();
                }
            });
        }


        public void ProposeLeaderBoard(Action callback = null)
        {
            UiLeagueLeaderBoardScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.LeagueLeaderBoard, onShowBegin: (showView) =>
            {
                if (showView is UiLeagueLeaderBoardScreen showUiLeagueLeaderBoardScreen)
                {
                    showUiLeagueLeaderBoardScreen.SetupController(this);
                    showUiLeagueLeaderBoardScreen.FillPlayers();
                    showUiLeagueLeaderBoardScreen.InitialScroll();
                }
            }, isForceHideIfExist: true) as UiLeagueLeaderBoardScreen;

            if (callback != null)
            {
                screen.onCloseScreen = () => callback.Invoke();
            }
        }


        public void ResetSkullsCountCollectOnLastLevel() =>
            SkullsCountCollectOnLastLevel = 0.0f;


        protected override void StartLiveOps()
        {
            LeaderBoard.StartNewBoard();

            base.StartLiveOps();
        }


        protected override void FinishLiveOps()
        {
            if (LeaderBoard.IsNextLeagueAchived(LeaderBoard.PlayerData.Id))
            {
                LeagueType league = LeaderBoard.LeagueType.GetNextLeague();
                
                LeagueReachController.SetupNewLeague(league);
            }

            IntermediateRewardController.ResetLeaguePoints();

            LeagueReachController.WriteLeagueLiveOpsId(CurrentLiveOpsId);


            liveOpsDurationTimer.Stop();
            
            CurrentLiveOpsId = string.Empty;
        }
        
        public void UiMainMenuScreen_OnFinishLeagueClaim() =>
            FinishAction();
        

        public void AddProposeListener(UnityAction callback)
        {
            OnLeaguePointsRecieved.AddListener(callback);
            NeedAnyPropose = true;
        }


        public void InvokeProposeListener()
        {
            OnLeaguePointsRecieved.Invoke();
            OnLeaguePointsRecieved.RemoveAllListeners();
        }


        public void MarkAsProposed() =>
             NeedAnyPropose = false;

        #endregion



        #region Events handlers

        private void CurrencyData_OnCurrencyAdded(CurrencyType currencyType, float value)
        {
            if (currencyType == CurrencyType.Skulls)
            {
                AddSkulls(value);
            }
        }
        
        #endregion
    }
}
