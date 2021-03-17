using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Utils;
using Drawmasters.Interfaces;
using Drawmasters.AbTesting;
using System;
using Drawmasters.Levels;


namespace Drawmasters.Proposal
{
    public abstract class LiveOpsEventController : IInitializable
    {
        #region Fields

        public event Action OnStarted;
        public event Action OnFinished;

        private readonly LiveOpsProposeController liveOpsProposeController;
        private readonly IHappyHoursAbSettings abSettings;

        private readonly RealtimeTimer durationTimer;

        #endregion



        #region Properties
        
        public bool WasActiveBeforeLevelStart { get; protected set; }

        public HappyHoursVisualSettings VisualSettings { get; }
        
        public IUaAbTestMechanic UaAbTestMechanic { get; }

        public string UiTimerSkeletonGraphicSkin => IsActive && WasForceShow ?
            IngameData.Settings.commonRewardSettings.eventActiveTimerSkin : IngameData.Settings.commonRewardSettings.eventDisabledTimerSkin;

        public abstract float PointsMultiplier { get; }

        private bool IsMechanicAvailable
        {
            get
            {
                bool isLiveOpsAvailable = liveOpsProposeController.IsMechanicAvailable;
                bool isEventAvailable = UaAbTestMechanic.WasAvailabilityChanged ?
                    UaAbTestMechanic.IsMechanicAvailable : abSettings.IsAvailable;

                return isLiveOpsAvailable && isEventAvailable;
            }
        }

        public bool IsActive =>
            IsMechanicAvailable &&
            durationTimer.IsTimerActive;


        private bool CanStartLiveOps =>
            IsMechanicAvailable &&
            LastStartedLiveOpsId != liveOpsProposeController.CurrentLiveOpsId &&
            liveOpsProposeController.IsActive &&
            !IsActive &&
            (float)liveOpsProposeController.TimeLeftLiveOps.TotalSeconds < abSettings.StartSecondsBeforeLiveOpsFinish;


        public bool CanForcePropose =>
            IsActive &&
            !WasForceShow;


        public bool WasForceShow
        {
            get => CustomPlayerPrefs.GetBool(WasForceShowKey, false);
            set => CustomPlayerPrefs.SetBool(WasForceShowKey, value);
        }


        private string LastStartedLiveOpsId
        {
            get => CustomPlayerPrefs.GetString(LastStartedLiveOpsIdKey, string.Empty);
            set => CustomPlayerPrefs.SetString(LastStartedLiveOpsIdKey, value);
        }

        #endregion



        #region Prefs Keys

        protected abstract string PrefsMainKey { get; }

        private string DurationTimerKey =>
            string.Concat(PrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixDurationTimerKey);

        private string WasForceShowKey =>
            string.Concat(PrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixForceShow);

        private string LastStartedLiveOpsIdKey =>
            string.Concat(PrefsMainKey, PrefsKeys.Proposal.LiveOpsEvent.LastStartedLiveOpsIdKey);

        #endregion



        #region Class lifecycle

        public LiveOpsEventController(LiveOpsProposeController _liveOpsProposeController,
                                      HappyHoursVisualSettings _visualSettings,
                                      IHappyHoursAbSettings _abSettings,
                                      IAbTestService _abTestService,
                                      ICommonStatisticsService _commonStatisticsService,
                                      IPlayerStatisticService _playerStatisticService,
                                      ITimeValidator _timeValidator)
        {
            liveOpsProposeController = _liveOpsProposeController;
            abSettings = _abSettings;
            VisualSettings = _visualSettings;
            
            string uaAbTestMechanicKey = string.Concat(PrefsKeys.AbTest.UaAbTestMechanicKeyPrefix, GetType().Name);
            UaAbTestMechanic = new CommonMechanicAvailability(uaAbTestMechanicKey);

            var eventStartTimer = new LoopedInvokeTimer(OnStartTimerUpdate);
            eventStartTimer.Start();

            durationTimer = new RealtimeTimer(DurationTimerKey, _timeValidator, false);
            durationTimer.SetTimeOverCallback(Finish);

            if (durationTimer.IsTimerActive)
            {
                durationTimer.Initialize();
                liveOpsProposeController.OnFinished += Finish;
            }

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            AttemptStart();

            if (!liveOpsProposeController.IsActive)
            {
                Finish();
            }
        }
        
        #endregion


        #region Public methods

        public void MarkForceProposed() =>
            WasForceShow = true;

        
        public virtual float GetPlayerMultipliedValue(float currentValue) =>
            currentValue * PointsMultiplier;

        
        #endregion



        #region Private methods

        private void AttemptStart()
        {
            if (CanStartLiveOps)
            {
                Start();
            }
        }


        private void Start()
        {
            if (!IsMechanicAvailable)
            {
                CustomDebug.Log("Event should be started. No comply with the live ops's conditions");
                return;
            }

            WasForceShow = false;
            LastStartedLiveOpsId = liveOpsProposeController.CurrentLiveOpsId;

            liveOpsProposeController.OnFinished += Finish;
            durationTimer.Start(abSettings.DurationSeconds);

            OnStarted?.Invoke();
        }


        private void Finish()
        {
            liveOpsProposeController.OnFinished -= Finish;

            durationTimer.Stop();
            
            OnFinished?.Invoke();
        }

        #endregion



        #region Events handlers

        private void OnStartTimerUpdate() =>
            AttemptStart();
        

        //TODO maxim.a move logic
        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                WasActiveBeforeLevelStart = IsActive && WasForceShow;
            }
        }

        #endregion
    }
}
