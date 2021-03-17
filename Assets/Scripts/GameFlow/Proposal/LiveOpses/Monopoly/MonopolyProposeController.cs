using System;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using Drawmasters.Ui;
using Drawmasters.Levels;
using Drawmasters.Utils.UiTimeProvider.Implementation;
using Drawmasters.Utils.UiTimeProvider.Interfaces;


namespace Drawmasters.Proposal
{
    public class MonopolyProposeController : LiveOpsProposeController
    {
        #region Fields

        public event Action OnAdsProposeRefresh;

        private readonly RealtimeTimer adsRefreshTimer;

        private readonly IAbTestService abTestService;
        
        private readonly ITimeUiTextConverter converter = new FlexibleUiTimerTimeConverter();

        #endregion



        #region Properties

        public MonopolySettings Settings { get; }

        public MonopolyVisualSettings VisualSettings { get; }

        public bool IsPlayerRollAvailable =>
            GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.RollBones) > 0;


        public bool IsVideoProposeAvailable =>
            !adsRefreshTimer.IsTimerActive;


        public string AdsReloadUiTimeLeft =>
            adsRefreshTimer.ConvertTime(converter);


        public override bool ShouldShowAlert
        {
            get
            {
                bool result = base.ShouldShowAlert;
                result &= IsPlayerRollAvailable;

                return result;
            }
        }


        public override bool IsActive =>
            base.IsActive &&
            !CollectedLapsReward(MonopolyLiveOpsDeskCounter).LastObject();


        public int MonopolyLiveOpsDeskCounter
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Proposal.MonopolyLiveOpsDeskCounter, default);
            private set => CustomPlayerPrefs.SetInt(PrefsKeys.Proposal.MonopolyLiveOpsDeskCounter, value);
        }

        public RewardData[] GeneratedLiveOpsReward =>
            GeneratedLiveOpsRewardSerialization.Data;


        public bool IsAutoRollEnabled
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.MonopolyAutoRoll, false);
            set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.MonopolyAutoRoll, value);
        }

        public override string LiveOpsAnalyticName => 
            LiveOpsNames.Monopoly.Name;

        public override string LiveOpsAnalyticEventId =>
            LiveOpsNames.Monopoly.GetEventName(GeneratedLiveOpsReward.LastObject());

        public override string LiveOpsAnalyticPosition => 
            string.Empty;

        protected override string LiveOpsPrefsMainKey =>
            PrefsKeys.Proposal.MonopolyMainKey;

        private RewardDataSerializationArray GeneratedLiveOpsRewardSerialization { get; }

        #endregion



        #region Class lifecycle

        public MonopolyProposeController(MonopolySettings _settings,
                                         MonopolyVisualSettings _visualSettings,
                                         IAbTestService _abTestService,
                                         ICommonStatisticsService _commonStatisticsService,
                                         IPlayerStatisticService _playerStatisticService,
                                         ITimeValidator _timeValidator) :
                                         base(_settings.LiveOpsProposeSettings(_abTestService.CommonData), _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            Settings = _settings;
            VisualSettings = _visualSettings;
            abTestService = _abTestService;

            adsRefreshTimer = new RealtimeTimer(PrefsKeys.Proposal.MonopolyBonesProposeTimer, _timeValidator);
            GeneratedLiveOpsRewardSerialization = new RewardDataSerializationArray(PrefsKeys.Proposal.LastMonopolyReward);
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            UiMonopolyScreen.OnShouldFinishLiveOps += FinishLiveOps;
        }


        public override void Propose()
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideScreenImmediately(ScreenType.MainMenu);
                LevelsManager.Instance.UnloadLevel();
                
                UiMonopolyScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.Monopoly, isForceHideIfExist: true) as UiMonopolyScreen;

                screen.SetDeskReward(Settings.GetCommonShowRewardPack());
                screen.SetMainReward(GeneratedLiveOpsReward);
                screen.RefreshVisual();
            });
        }


        public void IncrementDeskCounter() =>
            MonopolyLiveOpsDeskCounter++;


        public void MarkBonesAdsWatched()
        {
            adsRefreshTimer.Start(abTestService.CommonData.monopolyBonesForVideoReloadSeconds);
            adsRefreshTimer.SetTimeOverCallback(InvokeAdsRefresh);
            InvokeAdsRefresh();
        }


        private void InvokeAdsRefresh() =>
            OnAdsProposeRefresh?.Invoke();


        protected override void StartLiveOps()
        {
            GeneratedLiveOpsRewardSerialization.Data = Settings.GetRandomLapReward(ShowsCount + 1);
            MonopolyLiveOpsDeskCounter = default;

            if (!WasFirstLiveOpsStarted)
            {
                Settings.firstStartReward.Open();
                Settings.firstStartReward.Apply();
            }

            base.StartLiveOps();
        }


        protected override void FinishLiveOps()
        {
            liveOpsDurationTimer.Stop();

            UiMonopolyScreen screen = UiScreenManager.Instance.LoadedScreen<UiMonopolyScreen>(ScreenType.Monopoly);

            if (screen != null)
            {
                screen.AddOnHideCallback(FinishAction);
            }
            else
            {
                FinishAction();
            }
        }


        public bool[] CollectedLapsReward(int deskCounter)
        {
            int lapsesFinished = deskCounter / Settings.DescMovementsForLaps;

            bool[] result = new bool[Settings.CountsLapsForReward.Length];

            for (int i = 0; i < Settings.CountsLapsForReward.Length; i++)
            {
                result[i] = lapsesFinished >= Settings.CountsLapsForReward[i];
            }

            return result;
        }

        #endregion
    }
}
