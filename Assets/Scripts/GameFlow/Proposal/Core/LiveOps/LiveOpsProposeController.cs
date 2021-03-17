using System;
using Drawmasters.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Utils;
using Drawmasters.AbTesting;
using Drawmasters.Analytic;
using Drawmasters.Utils.UiTimeProvider.Implementation;
using Drawmasters.Utils.UiTimeProvider.Interfaces;


namespace Drawmasters.Proposal
{
    public abstract class LiveOpsProposeController :
        IProposalController,
        IForceProposal,
        IShowsCount,
        ILiveOpsFinishSoonNotification,
        ILiveOpsNextStartNotification
    {
        #region Fields

        public event Action OnStarted;
        public event Action OnFinished;

        protected readonly RealtimeTimer liveOpsDurationTimer;
        protected readonly RealtimeTimer liveOpsReloadTimer;

        protected readonly LiveOpsProposeSettings liveOpsProposeSettings;

        protected readonly ICommonStatisticsService commonStatisticsService;
        protected readonly IPlayerStatisticService playerStatisticService;
        protected readonly ITimeValidator timeValidator;
        protected readonly ITimeUiTextConverter defaultTimeConverter = new FlexibleUiTimerTimeConverter();

        #endregion



        #region Properties

        public IUaAbTestMechanic UaAbTestMechanic { get; }


        public bool IsMechanicAvailable =>
            UaAbTestMechanic.WasAvailabilityChanged ?
                UaAbTestMechanic.IsMechanicAvailable : 
                liveOpsProposeSettings.IsAvailable;


        public bool IsEnoughLevelsFinished
        {
            get
            {
                bool result = liveOpsProposeSettings.MinLevelForLiveOps == 0;
                result |= commonStatisticsService.LevelsFinishedCount >= liveOpsProposeSettings.MinLevelForLiveOps;

                return result;
            }
        }


        // maxim.ak what about case when it's not available, but timer's active?
        public virtual bool IsActive =>
            IsMechanicAvailable &&
            liveOpsDurationTimer.IsTimerActive;


        public bool CanForcePropose =>
            IsActive &&
            !IsLiveOpsReloading &&
            IsMechanicAvailable &&
            IsEnoughLevelsFinished &&
            IsAllConditionsForNextLiveOpsComplied &&
            !WasForceShow;
        

        public bool WasFirstLiveOpsStarted => ShowsCount > 0;


        protected virtual bool CanStartLiveOps =>
            !IsLiveOpsReloading &&
            IsMechanicAvailable &&
            IsEnoughLevelsFinished &&
            IsAllConditionsForNextLiveOpsComplied &&
            !IsActive;


        private bool IsAllConditionsForNextLiveOpsComplied =>
            IsEnoughDaysForNextLiveOpsPassed &&
            IsEnoughLevelsForNextLiveOpsPassed;
        

        private bool IsLiveOpsReloading =>
            liveOpsReloadTimer.IsTimerActive;


        public virtual bool ShouldShowAlert
        {
            get
            {
                bool result = IsMechanicAvailable;
                result &= IsActive;

                return result;
            }
        }

        public TimeSpan TimeLeftLiveOps =>
            liveOpsDurationTimer.TimeLeft;

        public string TimeUi =>
            IsActive ? 
                TimeLeftLiveOpsUi : 
                ReloadTimeLeftLiveOpsUi;


        public string LiveOpsHoursTimeUi =>
            IsActive ? 
                liveOpsDurationTimer.ConvertTime(defaultTimeConverter) : 
                liveOpsReloadTimer.ConvertTime(defaultTimeConverter);


        public string TimeLeftLiveOpsUi
        {
            get
            {
                string result = "Closing soon";

                if (IsActive)
                {
                    result = liveOpsDurationTimer.ConvertTime(defaultTimeConverter);
                }

                return result;
            }
        }


        private string ReloadTimeLeftLiveOpsUi
            => liveOpsReloadTimer.ConvertTime(defaultTimeConverter);


        private float NextLiveOpsWaitSeconds =>
            liveOpsProposeSettings.IsReloadTimeUsed ?
                liveOpsProposeSettings.ReloadTime +
                liveOpsProposeSettings.AvailabilitySettings.cooldownSeconds :
                liveOpsProposeSettings.AvailabilitySettings.cooldownSeconds;


        public string CurrentLiveOpsId
        {
            get => CustomPlayerPrefs.GetString(LiveOpsIdKey, "default_live_ops_id");
            protected set => CustomPlayerPrefs.SetString(LiveOpsIdKey, value);
        }

        public abstract string LiveOpsAnalyticName { get; }
        
        public abstract string LiveOpsAnalyticEventId { get; }
        
        public abstract string LiveOpsAnalyticPosition { get; }

        public bool IsCurrentLiveOpsTaskCompleted
        {
            get => CustomPlayerPrefs.GetBool(LiveOpsTaskCompletedKey, false);
            set => CustomPlayerPrefs.SetBool(LiveOpsTaskCompletedKey, value);
        }

        protected virtual string LiveOpsIdKey { get; } = "default_string";

        
        private DateTime NextLiveOpsDateTime
        {
            get => CustomPlayerPrefs.GetDateTime(NextLiveOpsDateTimeKey, DateTime.MaxValue);
            set => CustomPlayerPrefs.SetDateTime(NextLiveOpsDateTimeKey, value);
        }


        private int LastFinishedLevelsCount
        {
            get => CustomPlayerPrefs.GetInt(LastFinishedLevelsCountKey);
            set => CustomPlayerPrefs.SetInt(LastFinishedLevelsCountKey, value);
        }


        private bool WasForceShow
        {
            get => CustomPlayerPrefs.GetBool(WasForceShowKey, false);
            set => CustomPlayerPrefs.SetBool(WasForceShowKey, value);
        }


        private bool IsEnoughLevelsForNextLiveOpsPassed
        {
            get
            {
                int levelsDelta = commonStatisticsService.LevelsFinishedCount - LastFinishedLevelsCount;
                bool result = levelsDelta >= liveOpsProposeSettings.AvailabilitySettings.cooldownLevels;

                return result;
            }
        }


        private bool IsEnoughDaysForNextLiveOpsPassed
        {
            get
            {
                DateTime firstLaunchDate = commonStatisticsService.FirstLaunchDateTime.Date;
                int daysDelta = timeValidator.ValidNow.Date.Subtract(firstLaunchDate).Days;

                return daysDelta >= liveOpsProposeSettings.AvailabilitySettings.triggerDay;
            }
        }

        #endregion



        #region Prefs keys

        protected abstract string LiveOpsPrefsMainKey { get; }

        
        private string NextLiveOpsDateTimeKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixyNextLiveOpsDateTime);


        private string ShowCounterKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixShowCounter);


        private string WasForceShowKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixForceShow);


        private string TimerDurationKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixDurationTimerKey);

        
        private string TimerReloadKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixReloadTimerKey);

        
        protected string LastFinishedLevelsCountKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixLevelsDelta);

        protected string LiveOpsTaskCompletedKey =>
            string.Concat(LiveOpsPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixTaskCompleted);

        #endregion



        #region Class lifecycle

        public LiveOpsProposeController(LiveOpsProposeSettings _liveOpsProposeSettings,
                                        ICommonStatisticsService _commonStatisticsService,
                                        IPlayerStatisticService _playerStatisticService,
                                        ITimeValidator _timeValidator)
        {
            liveOpsProposeSettings = _liveOpsProposeSettings;
            commonStatisticsService = _commonStatisticsService;
            playerStatisticService = _playerStatisticService;
            timeValidator = _timeValidator;

            string uaAbTestMechanicKey = string.Concat(PrefsKeys.AbTest.UaAbTestMechanicKeyPrefix, GetType().Name);
            UaAbTestMechanic = new CommonMechanicAvailability(uaAbTestMechanicKey);

            liveOpsDurationTimer = new RealtimeTimer(TimerDurationKey, timeValidator, false);
            liveOpsDurationTimer.SetTimeOverCallback(FinishLiveOps);

            if (liveOpsDurationTimer.IsTimerActive)
            {
                liveOpsDurationTimer.Initialize();
            }

            liveOpsReloadTimer = new RealtimeTimer(TimerReloadKey, timeValidator, false);
            liveOpsReloadTimer.SetTimeOverCallback(AttemptStartProposal);

            if (liveOpsReloadTimer.IsTimerActive)
            {
                liveOpsReloadTimer.Initialize();
            }

            if (liveOpsReloadTimer.IsTimerActive &&
                liveOpsDurationTimer.IsTimerActive)
            {
                CustomDebug.Log($"Live ops {GetType().Name} reload and duration timers are active simultaneously. Wrong logic detected");
            }
        }

        #endregion


        #region Methods

        public abstract void Propose();

        
        public virtual void Initialize()
        {
            if (!liveOpsReloadTimer.IsTimerActive &&
                !liveOpsDurationTimer.IsTimerActive)
            {
                AttemptStartProposal();
            }

            DateTime now = timeValidator.ValidNow;

            // immitate start. It's correct logic
            if (CanStartLiveOps && now > NextLiveOpsDateTime)
            {
                if (!IsCurrentLiveOpsTaskCompleted)
                {
                    AnalyticHelper.SendLiveOpsFinishEvent(LiveOpsAnalyticName, LiveOpsAnalyticEventId, LiveOpsAnalyticPosition);    
                }

                liveOpsDurationTimer.Stop();
                AttemptStartProposal();
            }
            else if (CanStartLiveOps)
            {
                float secondsRest = WasFirstLiveOpsStarted ?
                    (float)NextLiveOpsDateTime.Subtract(now).TotalSeconds : NextLiveOpsWaitSeconds;

                liveOpsDurationTimer.Stop();
                liveOpsReloadTimer.Start(secondsRest);
            }

            UiLiveOpsPropose.OnShouldForceStart += UiLiveOpsPropose_OhShouldForceStartLiveOps;
        }


        public void MarkForceProposed() =>
            WasForceShow = true;


        public void ResetForceShowed() =>
            WasForceShow = false;


        public virtual void AttemptStartProposal()
        {
            if (CanStartLiveOps)
            {
                StartLiveOps();
            }
        }


        protected virtual void StartLiveOps()
        {
            if (!IsMechanicAvailable)
            {
                CustomDebug.Log("Live ops should be started. No comply with the live ops's conditions");
                return;
            }

            CurrentLiveOpsId = Guid.NewGuid().ToString();

            liveOpsReloadTimer.Stop();

            liveOpsDurationTimer.Start(liveOpsProposeSettings.DurationTime);

            NextLiveOpsDateTime = liveOpsDurationTimer.FinishTime;

            ShowsCount++;
            ResetForceShowed();

            OnStarted?.Invoke();

            IsCurrentLiveOpsTaskCompleted = false;
            AnalyticHelper.SendLiveOpsStartEvent(LiveOpsAnalyticName, LiveOpsAnalyticEventId);
        }


        protected virtual void FinishLiveOps()
        {
            liveOpsDurationTimer.Stop();

            FinishAction();
        }


        protected virtual void FinishAction()
        {
            liveOpsReloadTimer.Start(NextLiveOpsWaitSeconds);
            NextLiveOpsDateTime = liveOpsReloadTimer.FinishTime;
            LastFinishedLevelsCount = commonStatisticsService.LevelsFinishedCount;

            if (!IsCurrentLiveOpsTaskCompleted)
            {
                AnalyticHelper.SendLiveOpsFinishEvent(LiveOpsAnalyticName, LiveOpsAnalyticEventId, LiveOpsAnalyticPosition);    
            }
            
            OnFinished?.Invoke();
        }

        #endregion



        #region Events handlers

        private void UiLiveOpsPropose_OhShouldForceStartLiveOps(object anotherController)
        {
            if (this == anotherController)
            {
                AttemptStartProposal();
                MarkForceProposed();
            }
        }

        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(ShowCounterKey);
            set => CustomPlayerPrefs.SetInt(ShowCounterKey, value);
        }

        #endregion




        #region ILiveOpsFinishSoonNotification

        public virtual bool AllowRegisterFinishSoonNotification
        {
            get
            {
                bool result = true;

                result &= IsMechanicAvailable;
                result &= IsEnoughLevelsFinished;
                result &= IsActive;
                result &= WasFirstLiveOpsStarted;
                result &= NextLiveOpsDateTime < DateTime.MaxValue;

                if (IsActive)
                {
                    result &= FireDateTimeFinishSoonNotification > timeValidator.ValidNow;
                }

                return result;
            }
        }


        public virtual DateTime FireDateTimeFinishSoonNotification
        {
            get
            {
                if (!liveOpsDurationTimer.IsTimerActive)
                {
                    CustomDebug.Log("Attempt to get finish soon notification date time while event is not active");
                    
                    return timeValidator.ValidNow;
                }

                float seconds = liveOpsProposeSettings.NotificationSecondsBeforeLiveOpsFinish;
                return liveOpsDurationTimer.FinishTime.AddSeconds(-seconds);
            }
        }

        #endregion



        #region ILiveOpsNextStartNotification

        public virtual bool AllowRegisterNextStartNotification
        {
            get
            {
                bool result = true;

                result &= IsMechanicAvailable;
                result &= IsEnoughLevelsFinished;
                result &= IsAllConditionsForNextLiveOpsComplied;
                result &= IsActive;
                result &= WasFirstLiveOpsStarted;
                result &= NextLiveOpsDateTime < DateTime.MaxValue;

                if (IsActive)
                {
                    result &= FireDateTimeFinishSoonNotification > timeValidator.ValidNow;
                }

                return result;
            }
        }


        public DateTime FireDateTimeNextStartNotification =>
            NextLiveOpsDateTime;

        #endregion
    }
}
