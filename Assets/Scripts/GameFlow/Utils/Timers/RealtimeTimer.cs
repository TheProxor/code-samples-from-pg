using System;
using Drawmasters.Utils.UiTimeProvider.Interfaces;


namespace Drawmasters.Utils
{
    public class RealtimeTimer : 
        IInitializable,
        IDeinitializable
    {
        #region Fields

        private readonly string key;
        private readonly ITimeValidator timeValidator;
        private readonly bool isUtcOffset;

        private Action onTimeOverCallback;

        private bool shouldCheckForStop;

        #endregion



        #region Properties
        
        public TimeSpan TimeLeft => IsTimerActive ? 
            FinishTime.Subtract(DateTimeOffset) : 
            TimeSpan.Zero;

        public DateTime StartTime => CustomPlayerPrefs.GetDateTime(StartTimeKey, DateTimeOffset);

        public DateTime FinishTime => CustomPlayerPrefs.GetDateTime(FinishTimeKey);

        public bool IsTimeOver => TimeLeft.TotalSeconds <= 0.0d;

        public bool IsTimerActive => 
            CustomPlayerPrefs.HasKey(StartTimeKey) && 
            CustomPlayerPrefs.HasKey(FinishTimeKey);

        private string StartTimeKey => string.Concat(key, PrefsKeys.Utils.StartTimeKeyPostfix);

        private string FinishTimeKey => string.Concat(key, PrefsKeys.Utils.FinishTimeKeyPostfix);

        private DateTime DateTimeOffset =>
            isUtcOffset ? timeValidator.ValidUtcNow : timeValidator.ValidNow;

        #endregion



        #region Constructors

        public RealtimeTimer(string timerKey, 
            ITimeValidator _timeValidator, 
            bool _isUtcOffset = true)
        {
            key = timerKey;
            timeValidator = _timeValidator;
            isUtcOffset = _isUtcOffset;
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            MonoBehaviourLifecycle.OnUpdate += OnUpdate;
            shouldCheckForStop = true;
        }

        #endregion


        #region IDeinitializable

        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= OnUpdate;
            onTimeOverCallback = null;
        }

        #endregion



        #region Public methods

        public void Start(float duration)
        {
            if (IsTimerActive)
            {
                CustomDebug.Log("Trying to Start already active timer with key " + key);
            }

            SetUpStartTime();
            SetUpFinishTime(duration);
            Initialize();
        }

        
        public void Stop()
        {
            MonoBehaviourLifecycle.OnUpdate -= OnUpdate;
            shouldCheckForStop = false;

            CustomPlayerPrefs.DeleteKey(StartTimeKey);
            CustomPlayerPrefs.DeleteKey(FinishTimeKey);
        }
        

        public void SetTimeOverCallback(Action callback) =>
            onTimeOverCallback = callback;

        public T ConvertTime<T>(ITimeConverter<T> implementation)
            => implementation.Convert(this);

        #endregion
        
        
        
        #region Private methods

        private void SetUpStartTime() =>
            CustomPlayerPrefs.SetDateTime(StartTimeKey, DateTimeOffset);
        

        private void SetUpFinishTime(float seconds) =>
            CustomPlayerPrefs.SetDateTime(FinishTimeKey, DateTimeOffset.AddSeconds(seconds));
        
        #endregion



        #region Events handlers

        private void OnUpdate(float deltaTime)
        {
            if (shouldCheckForStop && IsTimeOver)
            {
                Stop();
                onTimeOverCallback?.Invoke();
            }
        }

        #endregion
    }
}
