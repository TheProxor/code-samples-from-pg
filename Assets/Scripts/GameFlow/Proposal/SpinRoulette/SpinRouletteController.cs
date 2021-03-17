using System;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Drawmasters.Utils.UiTimeProvider.Implementation;
using Drawmasters.Utils.UiTimeProvider.Interfaces;
using Modules.General;
using UnityEngine;


namespace Drawmasters
{
    public class SpinRouletteController : IProposalController, IShowsCount
    {
        #region Fields

        public event Action OnPreRewardRefresh;
        public event Action OnRewardRefreshed;
        public event Action OnFreeSpinned;

        public event Action OnStarted;
        public event Action OnFinished;

        private readonly RealtimeTimer refreshTimer;
        private readonly IMusicService musicService;
        private readonly IAbTestService abTestService;
        private readonly ICommonStatisticsService commonStatisticsService;

        private readonly RewardDataSerializationArray rewardDataSerializationArray;
        
        protected readonly ITimeUiTextConverter timeConverter = new FlexibleUiTimerTimeConverter();

        #endregion



        #region Properties

        public SpinRouletteSettings Settings { get; }

        public bool ShouldGetOnlyOneReward { get; private set; }


        public DateTime NextRefreshDate =>
            DateTime.Now.Add(refreshTimer.TimeLeft);


        public bool ShouldPlayTimerAnimation =>
            refreshTimer.TimeLeft.TotalSeconds <= Settings.secondsRestToStartAnimateTimer;


        public Color FindSegmentColor(int index) =>
            Settings.FindSegmentColor(index);


        public bool IsFreeSpinAvailable
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.IsRouletteFreeSpinAvailable, true);
            private set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.IsRouletteFreeSpinAvailable, value);
        }


        public string TimeUi =>
            refreshTimer.ConvertTime(timeConverter);


        public bool IsMechanicAvailable =>
            abTestService.CommonData.isSpinRouletteProposalAvailable;


        public bool IsEnoughLevelsFinished =>
            commonStatisticsService.LevelsFinishedCount >= abTestService.CommonData.LevelToForceShowRouletteSpin;


        public bool IsActive =>
            IsMechanicAvailable &&
            IsEnoughLevelsFinished &&
            WasRouletteOpened &&
            WasFirstLiveOpsStarted;


        public bool WasFirstLiveOpsStarted =>
            ShowsCount > 0;


        public bool ShouldShowAlert =>
            IsMechanicAvailable &&
            IsEnoughLevelsFinished &&
            !CanForcePropose &&
            IsFreeSpinAvailable;


        private RewardData[] LastRewardData
        {
            get => rewardDataSerializationArray.Data;
            set => rewardDataSerializationArray.Data = value;
        }


        private bool WasRouletteOpened =>
            CustomPlayerPrefs.HasKey(PrefsKeys.Proposal.LastSpinRouletteReward);


        #endregion



        #region Class lifecycle

        public SpinRouletteController(SpinRouletteSettings _spinRouletteSettings,
                                      IMusicService _musicService,
                                      IAbTestService _abTestService,
                                      ICommonStatisticsService _commonStatisticsService,
                                      ITimeValidator _timeValidator)
        {
            Settings = _spinRouletteSettings;

            refreshTimer = new RealtimeTimer(PrefsKeys.Proposal.SpinRouletteTimer, _timeValidator);
            refreshTimer.SetTimeOverCallback(OnTimerRefreshOver);
            refreshTimer.Initialize();

            musicService = _musicService;
            abTestService = _abTestService;
            commonStatisticsService = _commonStatisticsService;
            ShouldGetOnlyOneReward = false;

            rewardDataSerializationArray = new RewardDataSerializationArray(PrefsKeys.Proposal.LastSpinRouletteReward);

            // Logic for old users
            if (ShowsCount > 0)
            {
                RewardData[] actualRewardData = LastRewardData;
                if (actualRewardData != null)
                {
                    bool wasLegacyRewardFound = RewardDataUtility.IsLegacyReward(actualRewardData);

                    if (wasLegacyRewardFound)
                    {
                        OnShouldRefreshRoulette();
                    }
                }
            }
        }

        #endregion



        #region Propose Methods

        public void Propose(Action<bool> onProposeHidded = default)
        {
            ShouldGetOnlyOneReward = false;

            bool CanPropose = IsMechanicAvailable &&
            IsEnoughLevelsFinished;

            if (CanPropose)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.SpinRoulette, onShowBegin: showedView =>
                {
                    if (showedView is SpinRouletteScreen spinRouletteScreen)
                    {
                        if (!WasRouletteOpened)
                        {
                            OnShouldRefreshRoulette();
                        }

                        spinRouletteScreen.SetReward(LastRewardData);
                        spinRouletteScreen.RouletteWheelSprite = Settings.rouletteWheelSprite;
                    }
                },
                onShowed: (showedView) => musicService.SetMusicVolume(0.6f),
                onHided: (view) => musicService.SetMusicVolume(1f),
                onHideBegin: (view) => onProposeHidded?.Invoke(true));
            }
            else
            {
                onProposeHidded?.Invoke(false);
            }
        }
        
        
        public void InstantPropose(Action hidedCallback, SpinRouletteSettings sequenceRewardPackSettings, int showIndex = -1, Action proposalEndCallback = default)
        {
            ShouldGetOnlyOneReward = true;
            
            UiScreenManager.Instance.ShowScreen(ScreenType.SpinRoulette, onShowBegin: showedView =>
                {
                    if (showedView is SpinRouletteScreen spinRouletteScreen)
                    {
                        RewardData[] rewardData = sequenceRewardPackSettings.GetRewardPack(showIndex);
                        spinRouletteScreen.SetReward(rewardData);
                        spinRouletteScreen.RouletteWheelSprite = sequenceRewardPackSettings.rouletteWheelSprite;
                    }
                },
                onHided: view => hidedCallback?.Invoke(),
                onHideBegin: hiveView => proposalEndCallback?.Invoke());
        }

        #endregion



        #region IForceProposal

        public bool CanForcePropose
        {
            get
            {
                bool isForceProposeLevel = commonStatisticsService.LevelsFinishedCount == abTestService.CommonData.LevelToForceShowRouletteSpin;

                return IsMechanicAvailable &&
                       IsEnoughLevelsFinished &&
                       isForceProposeLevel &&
                       !WasProposalTutorialShown;
            }
        }


        private bool WasProposalTutorialShown
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.PlayerInfo.WasSpinProposeTutorialShown, false);
            set => CustomPlayerPrefs.SetBool(PrefsKeys.PlayerInfo.WasSpinProposeTutorialShown, value);
        }

        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Proposal.SpinRouletteShowCounter);
            set => CustomPlayerPrefs.SetInt(PrefsKeys.Proposal.SpinRouletteShowCounter, value);
        }

        #endregion



        #region Methods

        public void MarkSpinedFree()
        {
            IsFreeSpinAvailable = false;
            OnFreeSpinned?.Invoke();
        }


        public void MarkForceProposed() =>
            WasProposalTutorialShown = true;


        public void AttemptStartProposal() { }


        private void OnTimerRefreshOver()
        {
            ShowsCount++;
            OnShouldRefreshRoulette();
        }


        private void OnShouldRefreshRoulette()
        {
            OnPreRewardRefresh?.Invoke();

            LastRewardData = Settings.GetRewardPack(ShowsCount - 1);
            IsFreeSpinAvailable = true;

            if (UiScreenManager.Instance.IsScreenActive(ScreenType.SpinRoulette))
            {
                var screen = UiScreenManager.Instance.LoadedScreen<SpinRouletteScreen>(ScreenType.SpinRoulette);

                RewardData[] actualRewardData = LastRewardData;

                foreach (var data in actualRewardData)
                {
                    if (data is CurrencyReward currencyRewardData && !currencyRewardData.currencyType.IsAvailableForShow())
                    {
                        currencyRewardData.currencyType = IngameData.Settings.coinLevelObjectSettings.mansionReplacedCurrencyType;
                    }
                }

                LastRewardData = actualRewardData;

                screen.SetReward(LastRewardData, Settings.refreshRewardDelayForSpinningState);

                Scheduler.Instance.CallMethodWithDelay(this, 
                    RestartTimer, 
                    Settings.refreshRewardDelayForSpinningState + Settings.refreshSpinSettings.rotateDuration);
            }
            else
            {
                RestartTimer();
            }

            OnRewardRefreshed?.Invoke();
        }


        public float AngleForSegment(int elementIndexToSpin) =>
            Settings.AngleForSegment(elementIndexToSpin);


        private void RestartTimer()
        {
            float seconds = abTestService.CommonData.FreeRouletteSpinSeconds;
            refreshTimer.Start(seconds);
        }

        #endregion
    }
}
