using System;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using Drawmasters.Utils.UiTimeProvider.Implementation;
using Drawmasters.Utils.UiTimeProvider.Interfaces;


namespace Drawmasters.OffersSystem
{
    public abstract class BaseOffer : IProposalController, IOffer, IShowsCount, IForceProposalOffer
    {
        #region Fields

        protected BaseOfferTrigger[] triggers;
        
        protected readonly RealtimeTimer offerDurationTimer;
        protected readonly RealtimeTimer offerCooldownTimer;

        protected readonly AbOfferSettings offerSettings;

        protected readonly ICommonStatisticsService commonStatisticsService;
        protected readonly IPlayerStatisticService playerStatisticService;
        protected readonly ITimeValidator timeValidator;

        private static BaseOffer mutexObject = null;
        
        private readonly ITimeUiTextConverter converter = new FlexibleUiTimerTimeConverter();

        #endregion



        #region Properties

        protected virtual bool WasForceProposed
        {
            get => CustomPlayerPrefs.GetBool(WasForceProposedKey);
            set => CustomPlayerPrefs.SetBool(WasForceProposedKey, value);
        }

        #endregion



        #region IProposalController

        public event Action OnStarted;
        public event Action OnFinished;

        public bool IsEnoughLevelsFinished =>
            true;

        public bool CanForcePropose =>
            IsActive &&
            IsMechanicAvailable &&
            IsEnoughLevelsFinished &&
            !WasForceProposed;


        public bool WasFirstLiveOpsStarted =>
            ShowsCount > 0;

        public bool ShouldShowAlert =>
            false;


        public string TimeUi =>
            IsActive ? 
                offerDurationTimer.ConvertTime(converter) : 
                string.Empty;

        public TimeSpan TimeLeftOffer =>
            offerDurationTimer.TimeLeft;


        public void MarkForceProposed() =>
            WasForceProposed = true;


        public void AttemptStartProposal()
        {
            if (CanStartOffer)
            {
                StartOffer();
            }
        }

        #endregion



        #region IOffer property

        public string UiTimerSkeletonGraphicSkin => IsActive && !CanForcePropose ?
            IngameData.Settings.commonRewardSettings.eventActiveTimerSkin : IngameData.Settings.commonRewardSettings.eventDisabledTimerSkin;


        public abstract string OfferType { get; }
        
        
        public string OfferId { get; }
        
        
        public bool IsMechanicAvailable =>
            UaAbTestMechanic.WasAvailabilityChanged ? 
                UaAbTestMechanic.IsMechanicAvailable : 
                offerSettings.IsAvailable;

        
        public virtual bool IsActive =>
            IsMechanicAvailable &&
            IsAllTriggersActive &&
            offerDurationTimer.IsTimerActive &&
            commonStatisticsService.IsIapsAvailable;

        
        public bool IsAllTriggersActive
        {
            get
            {
                bool result = true;

                foreach (BaseOfferTrigger trigger in triggers)
                {
                    result &= trigger.IsActive;
                }

                return result;
            }
        }


        public float DurationTime =>
            offerSettings.DurationTime;

        
        public string InAppId =>
            offerSettings.IapID;

        
        public float OfferCooldownTime =>
            offerSettings.CooldownTime;

        #endregion


        #region IForceProposalOffer

        public abstract void ForcePropose(string entryPoint, Action onProposeHidden = default);

        public virtual void ForcePropose(Action onProposeHidden = default)
        {
            ForcePropose(OfferKeys.EntryPoint.Auto, onProposeHidden);
        }
        

        #endregion



        #region Properties

        public string EntryPoint { get; protected set; }
        
        
        public BaseOffer MutexObject => mutexObject;

        public IUaAbTestMechanic UaAbTestMechanic { get; }

        public bool WasFirstOfferStarted => ShowsCount > 0;
        
        
        protected virtual bool CanStartOffer =>
            !IsOfferCooldown &&
            IsMechanicAvailable &&
            IsAllTriggersActive &&
            !IsActive &&
            commonStatisticsService.IsIapsAvailable;

        
        private bool IsOfferCooldown =>
            offerCooldownTimer.IsTimerActive;
        
        
        private DateTime NextRunOfferDateTime
        {
            get => CustomPlayerPrefs.GetDateTime(NextRunOfferDateTimeKey, DateTime.MaxValue);
            set => CustomPlayerPrefs.SetDateTime(NextRunOfferDateTimeKey, value);
        }
        
        
        private float NextOfferWaitSeconds =>
            offerSettings.CooldownTime;

        
        #endregion

        
        
        #region Prefs keys
        
        protected abstract string OfferPrefsMainKey { get; }

        public virtual string ShowCounterKey =>
            string.Concat(OfferPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixShowCounter);

        
        private string NextRunOfferDateTimeKey =>
            string.Concat(OfferPrefsMainKey, PrefsKeys.Offer.BaseOffer.PostfixyNextRunOfferDateTime);

        
        private string TimerDurationKey =>
            string.Concat(OfferPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixDurationTimerKey);

        
        private string TimerReloadKey =>
            string.Concat(OfferPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixReloadTimerKey);


        private string WasForceProposedKey =>
            string.Concat(OfferPrefsMainKey, PrefsKeys.Proposal.LiveOps.PostfixForceShow);

        #endregion



        #region Ctor

        protected BaseOffer(AbOfferSettings _offerSettings,
                        ICommonStatisticsService _commonStatisticsService,
                        IPlayerStatisticService _playerStatisticService,
                        ITimeValidator _timeValidator)
        {
            offerSettings = _offerSettings;
            commonStatisticsService = _commonStatisticsService;
            playerStatisticService = _playerStatisticService;
            timeValidator = _timeValidator;

            OfferId = Guid.NewGuid().ToString();
            
            string uaAbTestMechanicKey = string.Concat(PrefsKeys.AbTest.UaAbTestMechanicKeyPrefix, GetType().Name);
            UaAbTestMechanic = new CommonMechanicAvailability(uaAbTestMechanicKey);
            
            offerDurationTimer = new RealtimeTimer(TimerDurationKey, timeValidator, false);
            offerDurationTimer.SetTimeOverCallback(FinishOffer);

            offerCooldownTimer = new RealtimeTimer(TimerReloadKey, timeValidator, false);
            offerCooldownTimer.SetTimeOverCallback(AttemptStartProposal);
        }
        
        #endregion
        
        
        
        #region Methods
        
        public virtual void Initialize()
        {
            if (offerCooldownTimer.IsTimerActive)
            {
                offerCooldownTimer.Initialize();
            }

            if (offerDurationTimer.IsTimerActive)
            {
                offerDurationTimer.Initialize();
            }

            if (offerCooldownTimer.IsTimerActive &&
                offerDurationTimer.IsTimerActive)
            {
                CustomDebug.Log($"Offer {GetType().Name} reload and duration timers are active simultaneously. Wrong logic detected");
            }

            InitializeTriggers();
            
            if (!offerCooldownTimer.IsTimerActive &&
                !offerDurationTimer.IsTimerActive)
            {
                AttemptStartProposal();
            }

            DateTime now = timeValidator.ValidNow;

            // immitate start. It's correct logic
            if (CanStartOffer && now > NextRunOfferDateTime)
            {
                offerDurationTimer.Stop();
                AttemptStartProposal();
            }
            else if (CanStartOffer)
            {
                float secondsRest = WasFirstOfferStarted ?
                    (float)NextRunOfferDateTime.Subtract(now).TotalSeconds : NextOfferWaitSeconds;

                offerDurationTimer.Stop();
                offerCooldownTimer.Start(secondsRest);
            }
        }


        public virtual void InitializeTriggers()
        {
            triggers = CreateTrigers(offerSettings.Triger);

            foreach (var trigger in triggers)
            {
                trigger.Initialize();
                trigger.OnActivated += Triger_OnActivated;
            }
        }


        protected abstract BaseOfferTrigger[] CreateTrigers(string triger);
        

        protected virtual void StartOffer()
        {
            if (!IsMechanicAvailable)
            {
                CustomDebug.Log("Live ops should be started. No comply with the live ops's conditions");
                return;
            }

            WasForceProposed = false;

            ShowsCount++;

            if (!IsAllTriggersActive)
            {
                return;
            }
            
            mutexObject = this;
            
            offerCooldownTimer.Stop();

            offerDurationTimer.Start(offerSettings.DurationTime);

            NextRunOfferDateTime = offerDurationTimer.FinishTime;
            
            OnStarted?.Invoke();
        }

        protected virtual void FinishOffer()
        {
            offerDurationTimer.Stop();
            mutexObject = null;
            
            FinishAction();
        }
        
        
        protected void FinishAction()
        {
            offerCooldownTimer.Start(NextOfferWaitSeconds);
            NextRunOfferDateTime = offerCooldownTimer.FinishTime;
            
            OnFinished?.Invoke();
        }

        #endregion


        #region Events handlers
        
        protected void Triger_OnActivated() =>
            AttemptStartProposal();
        
        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(ShowCounterKey);
            set => CustomPlayerPrefs.SetInt(ShowCounterKey, value);
        }

        #endregion
    }
}