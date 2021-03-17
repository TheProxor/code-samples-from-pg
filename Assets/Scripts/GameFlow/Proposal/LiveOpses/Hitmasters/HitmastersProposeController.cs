using System;
using System.Linq;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Levels.Order;
using UnityEngine;
using Drawmasters.Utils;
using Drawmasters.Ui;
using Drawmasters.Ui.Hitmasters;


namespace Drawmasters.Proposal
{
    public class HitmastersProposeController : LiveOpsProposeController
    {
        #region Fields

        private readonly IAbTestService abTestService;

        #endregion



        #region Properties

        private HitmastersSettings Settings { get; }
        
        public HitmastersProposals HitmastersProposals { get; private set; }

        public HitmastersVisualSettings VisualSettings { get; }

        public HitmastersLiveOpsLevelOrderService LevelOrderService { get; }

        public bool IsCurrentLiveOpsCompleted =>
            IsModeCompleted(LiveOpsGameMode);


        public override bool IsActive =>
            base.IsActive &&
            !IsModeCompleted(LiveOpsGameMode);

        
        public override bool ShouldShowAlert => false;

        public GameMode LiveOpsGameMode
        {
            get => CustomPlayerPrefs.GetEnumValue<GameMode>(PrefsKeys.Proposal.HitmastersSpinOffLiveOpsGameMode);
            private set => CustomPlayerPrefs.SetEnumValue(PrefsKeys.Proposal.HitmastersSpinOffLiveOpsGameMode, value);
        }
        

        public bool ShouldShowPreviewScreen
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.HitmastersShouldShowPreviewScreen, default);
            set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.HitmastersShouldShowPreviewScreen, value);
        }


        public bool ShouldScrollMapOnShow
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.HitmastersShouldScrollMapOnShow);
            private set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.HitmastersShouldScrollMapOnShow, value);
        }


        public int LiveOpsLevelCounter =>
            CurrentModeAndIndexToPlay.mode.GetFinishedLevels();


        public RewardData GeneratedLiveOpsReward =>
            GeneratedLiveOpsRewardSerialization.Data;

        private RewardDataSerialization GeneratedLiveOpsRewardSerialization { get; }

        private GameMode[] LoadedGameModeSequence { get; }

        public (GameMode mode, int index) CurrentModeAndIndexToPlay
        {
            get
            {
                (GameMode, int) result = (default, default);

                bool isLiveOpsFinished = IsAllModesCompleted(LoadedGameModeSequence);

                if (isLiveOpsFinished)
                {
                    CustomDebug.Log($"Live ops is already finished");
                    return result;
                }

                result.Item1 = LiveOpsGameMode;
                result.Item2 = result.Item1.GetCurrentLevelIndex();

                return result;
            }
        }

        public override string LiveOpsAnalyticName => 
            LiveOpsNames.Hitmasters.Name;

        public override string LiveOpsAnalyticEventId =>
            LiveOpsNames.Hitmasters.GetEventName(LiveOpsGameMode);

        public override string LiveOpsAnalyticPosition => 
            string.Empty;

        protected override string LiveOpsPrefsMainKey =>
            PrefsKeys.Proposal.HitmastersSpinOffMainKey;

        #endregion



        #region Class lifecycle

        public HitmastersProposeController(HitmastersSettings _settings,
                                           HitmastersVisualSettings _visualSettings,
                                           LevelsOrder levelsOrder,
                                           IAbTestService _abTestService,
                                           ICommonStatisticsService _commonStatisticsService,
                                           IPlayerStatisticService _playerStatisticService,
                                           ITimeValidator _timeValidator) :
            base(_settings.LiveOpsProposeSettings(_abTestService.CommonData), _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            PrefsUtility.TryRestoreInt(PrefsKeys.Proposal.HotmastersLevelsDelta, LastFinishedLevelsCountKey);

            Settings = _settings;
            VisualSettings = _visualSettings;
            abTestService = _abTestService;

            GeneratedLiveOpsRewardSerialization = new RewardDataSerialization(PrefsKeys.Proposal.HitmastersSpinOffLastReward);
            LevelOrderService = new HitmastersLiveOpsLevelOrderService(levelsOrder, playerStatisticService, abTestService);

            LoadedGameModeSequence = abTestService.CommonData.hitmastersSpinOffModesSequence
                .Where(e => Enum.IsDefined(typeof(GameMode), e))
                .Select(e =>
                {
                    bool wasParseSuccess = Enum.TryParse(e, out GameMode mode);

                    if (!wasParseSuccess)
                    {
                        CustomDebug.Log($"Wrong ab test parameter {e}");
                    }

                    return mode;
                })
                .ToArray();
        }

        #endregion



        #region Methods

        public override void Propose()
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);

                (GameMode mode, int index) = CurrentModeAndIndexToPlay;

                bool isProposeAvailable = 
                    HitmastersProposals.IsAnyProposalAvailable(
                        mode, 
                        LiveOpsLevelCounter + 1, 
                        out HitmastersProposals.Proposal p);

                if (isProposeAvailable)
                {
                    HitmastersProposals.LoadProposal(mode, LiveOpsLevelCounter + 1);
                }
                else
                {
                    LevelsManager.Instance.UnloadLevel();
                    LevelsManager.Instance.LoadLevel(mode, index);

                    LevelsManager.Instance.PlayLevel();
                }
            });
        }


        public override void Initialize()
        {
            base.Initialize();

            HitmastersProposals = new HitmastersProposals(abTestService, GameServices.Instance.ProposalsLoader);

            UiHitmastersMapScreen.OnShouldFinishLiveOps += FinishLiveOps;
        }


        protected override void StartLiveOps()
        {
            LiveOpsGameMode = GetNextGameModeToPlay(LiveOpsGameMode);
            GeneratedLiveOpsRewardSerialization.Data = Settings.GetCommonShowReward(LiveOpsGameMode);

            ShouldScrollMapOnShow = true;
            ShouldShowPreviewScreen = true;

            base.StartLiveOps();
        }


        public void MarkMapOnShowScrolled() =>
            ShouldScrollMapOnShow = false;


        public GameMode GetNextGameModeToPlay(GameMode oldMode)
        {
            GameMode result = default;

            bool isLiveOpsFinished = IsAllModesCompleted(LoadedGameModeSequence);

            if (isLiveOpsFinished)
            {
                ClearProgress();
            }

            GameMode[] modes = Array.FindAll(LoadedGameModeSequence, e => !IsModeCompleted(e) && !IsModeStarted(e));

            if (modes.Length > 1)
            {
                if (Settings.IsAllRewardsClaimed())
                {
                    result = Array.FindAll(modes, e => e != oldMode).RandomObject();
                }
                else
                {
                    result = modes.First(e => e != oldMode);
                }
            }
            else
            {
                result = LoadedGameModeSequence.FirstOrDefault(e => !IsModeCompleted(e));
                
                ClearProgress(result);
            }

            return result;
        }


        private bool IsAllModesCompleted(GameMode[] gameModes) =>
            !Array.Exists(gameModes, e => !IsModeCompleted(e));


        private bool IsModeCompleted(GameMode gameMode)
        {
            if (!gameMode.IsHitmastersLiveOps())
            {
                CustomDebug.Log($"Wrong game mode {gameMode}");
                return false;
            }

            return LevelOrderService.IsIndexOverflow(gameMode, gameMode.GetCurrentLevelIndex());
        }


        private bool IsModeStarted(GameMode gameMode)
        {
            if (!gameMode.IsHitmastersLiveOps())
            {
                CustomDebug.Log($"Wrong game mode");
                return false;
            }

            return gameMode.GetCurrentLevelIndex() > 0;
        }


        private void ClearProgress()
        {
            foreach (var mode in LoadedGameModeSequence)
            {
                ClearProgress(mode);
            }
        }


        private void ClearProgress(GameMode mode)
        {
            playerStatisticService.PlayerData.SetModeInfo(mode, 0);
            mode.ResetFinishedLevels();
        }

        #endregion
    }
}
