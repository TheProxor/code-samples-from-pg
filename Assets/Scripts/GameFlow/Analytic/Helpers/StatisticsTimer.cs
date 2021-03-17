using System;
using System.Collections;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;
using UnityEngine.Networking;


namespace Drawmasters
{
    public class StatisticsTimer : IDeinitializable
    {
        #region Fields

        public static event Action<int> OnDaysPassed;

        private const string SERVER_URL = "https://api.playgendary.com/v1/info/time?build=";
        private const string TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";

        private const float MINUTES_IN_GAME_ACCURACY = 0.001f;

        private bool shouldSkipNextFrame;
        private bool isApplicationInFocus;
        private float currentGameSecondInFocus;

        private TimeSpan cheatedTime;

        private readonly IBackgroundService backgroundService;

        #endregion



        #region Properties

        public DateTime RealUtcTime => DateTime.UtcNow.Subtract(cheatedTime);


        public DateTime LastRealUtcTime
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.PlayerInfo.LastRealUTCDate, DateTime.MinValue);
            private set => CustomPlayerPrefs.SetDateTime(PrefsKeys.PlayerInfo.LastRealUTCDate, value);
        }


        public float TotalMinutesInGame
        {
            get
            {
                float result = SavedGameMinutes + currentGameSecondInFocus / 60.0f;
                result = CommonUtility.RoundToValue(result, MINUTES_IN_GAME_ACCURACY);

                return result;
            }
        }


        private float SavedGameMinutes
        {
            get => CustomPlayerPrefs.GetFloat(PrefsKeys.Analytics.MinutesInGame, 0.0f);
            set => CustomPlayerPrefs.SetFloat(PrefsKeys.Analytics.MinutesInGame, value);
        }


        private DateTime LastActiveDay
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Analytics.LastActiveDay, CustomPlayerPrefs.DEFAULT_DATE_TIME);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Analytics.LastActiveDay, value);
        }

        #endregion



        #region Ctor

        public StatisticsTimer(IBackgroundService _backgroundService)
        {
            backgroundService = _backgroundService;
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            backgroundService.OnApplicationSuspend += BackgroundService_OnApplicationSuspend;            

            MonoBehaviourLifecycle.PlayCoroutine(CheckServerTime());
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            backgroundService.OnApplicationSuspend -= BackgroundService_OnApplicationSuspend;
        }

        #endregion



        #region IEnumerators

        private IEnumerator CheckServerTime()
        {
            var info = new UnityWebRequest(SERVER_URL + Application.version);
            info.downloadHandler = new DownloadHandlerBuffer();

            yield return info.SendWebRequest();

            if (string.IsNullOrEmpty(info.error))
            {
                string serverInfo = MiniJSON.JsonConvert.DeserializeObject<string>(info.downloadHandler.text);

                if (!string.IsNullOrEmpty(serverInfo))
                {
                    cheatedTime = DateTime.Now - DateTime.ParseExact(serverInfo, TIME_FORMAT, null);

                    LastRealUtcTime = DateTime.UtcNow.Subtract(cheatedTime);
                }
            }
        }

        #endregion



        #region Methods

        private void CheckActiveDays()
        {
            if (LastActiveDay == CustomPlayerPrefs.DEFAULT_DATE_TIME)
            {
                LastActiveDay = DateTime.Now;
            }
            else
            {
                TimeSpan spanBeforeLastDay = DateTime.Now - LastActiveDay;

                int daysPassed = spanBeforeLastDay.Days;

                if (daysPassed >= 1)
                {
                    OnDaysPassed?.Invoke(daysPassed);

                    LastActiveDay = DateTime.Now;
                }
            }
        }


        private void SaveMinutesInGame()
        {
            SavedGameMinutes = TotalMinutesInGame;
            currentGameSecondInFocus = 0.0f;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (isApplicationInFocus)
            {
                if (shouldSkipNextFrame)
                {
                    shouldSkipNextFrame = false;
                }
                else
                {
                    currentGameSecondInFocus += Time.unscaledDeltaTime;
                }
            }
        }


        private void BackgroundService_OnApplicationSuspend(bool isEnteredBackground, TimeSpan timeSpan)
        {
            MonoBehaviourLifecycle.PlayCoroutine(CheckServerTime());

            CheckActiveDays();

            if (isEnteredBackground)
            {
                SaveMinutesInGame();
                isApplicationInFocus = false;
            }
            else
            {
                isApplicationInFocus = true;
                shouldSkipNextFrame = true;
            }
        }

        #endregion
    }
}
