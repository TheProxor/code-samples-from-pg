using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Modules.Networking;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Utils
{
    public class TimeValidation : ITimeValidator
    {
        #region Fields

        private const string ParsingFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        private TimeValidationStatus currentStatus;

        private readonly IBackgroundService backgroundService;
        private readonly string url;
        private readonly DateTime unixEpoch;

        private double validStartupTimeStamp;

        #endregion



        #region Class lifecycle

        public TimeValidation(IBackgroundService _backgroundService, string _url)
        {
            backgroundService = _backgroundService;
            url = _url;
            unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            DateTime utcNow = DateTime.UtcNow;
            DateTime validDeviceDateTime = utcNow < LastBackgroundEnterRealUtcTime ? LastBackgroundEnterRealUtcTime : utcNow;
            LastBackgroundEnterRealUtcTime = ValidUtcNow;

            validStartupTimeStamp = CalculateTimestamp(validDeviceDateTime);

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                // TODO: logic for timestamp setup without internet?
            }

            currentStatus = TimeValidationStatus.NotRequested;
            Validate();
        }

        #endregion



        #region ITimeValidatable

        public void Initialize()
        {
            Application.logMessageReceived += Application_logMessageReceived;
            backgroundService.OnApplicationSuspend += BackgroundService_OnApplicationSuspend;
        }


        public void Deinitialize()
        {
            backgroundService.OnApplicationSuspend -= BackgroundService_OnApplicationSuspend;
            Application.logMessageReceived -= Application_logMessageReceived;
        }


        public bool WasValidated =>
            currentStatus == TimeValidationStatus.GotResponse;

        public DateTime ValidNow =>
            ValidUtcNow.ToLocalTime();

        public DateTime ValidUtcNow
        {
            get
            {
                if (AllowTimeCheating)
                {
                    return CheatUtcNow;
                }

                double currentTimeStamp = validStartupTimeStamp + Time.realtimeSinceStartup;
                DateTime result = ConvertFromUnixTimestamp(currentTimeStamp);

                return result;
            }
        }


        public bool AllowTimeCheating { get; set; } = true;


        public DateTime CheatUtcNow =>
            DateTime.UtcNow;


        public DateTime LastBackgroundEnterRealUtcTime
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Application.TimeValidationLastBackgroundEnterRealUtcTime, DateTime.MinValue);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Application.TimeValidationLastBackgroundEnterRealUtcTime, value);
        }

        #endregion



        #region Methods

        private async void Validate(Action callback = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ReachabilityHandler.Instance.NetworkReachabilityStatusChanged += ReachabilityHandler_NetworkReachabilityStatusChanged;
            }
            else
            {
                await CheckServerTime(callback);
            }
        }


        private async Task CheckServerTime(Action callback = null)
        {
            currentStatus = TimeValidationStatus.WaitingResponse;

            UnityWebRequest request = new UnityWebRequest(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbGET;
            request.timeout = 5;

            UnityWebRequestAsyncOperation requestAsyncOperation = request.SendWebRequest();
            try
            {
                while (!requestAsyncOperation.isDone)
                {
                    await Task.Yield();
                }
            }
            catch
            {
                CustomDebug.LogError($"Exception found. Error: {request.error}");
                request.Abort();
            }

            if (request.isDone && request.responseCode == 200)
            {
                if (string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    CustomDebug.LogError("Data cannot be handled");
                }
                else
                {
                    currentStatus = TimeValidationStatus.GotResponse;
                    OnGotResponse(request.downloadHandler.text);
                    LastBackgroundEnterRealUtcTime = ValidUtcNow;

                    callback?.Invoke();
                }
            }
            else if (request.isNetworkError)
            {
                CustomDebug.LogError("Network exception");
            }
        }


        private double CalculateTimestamp(DateTime currentDateTime) =>
            ConvertToUnixTimestamp(currentDateTime) - Time.realtimeSinceStartup;


        private double ConvertToUnixTimestamp(DateTime date)
        {
            TimeSpan diff = date.ToUniversalTime().Subtract(unixEpoch);
            return Math.Floor(diff.TotalSeconds);
        }


        private DateTime ConvertFromUnixTimestamp(double timestamp) =>
            unixEpoch.AddSeconds(timestamp);

        #endregion



        #region Events handlers

        private void OnGotResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                Debug.Log($"Incorrect Response");
                return;
            }

            string fetchedString = response.Replace("\"", "");

            bool isParsedSuccessfully = DateTime.TryParseExact(fetchedString,
                                                               ParsingFormat,
                                                               null,
                                                               DateTimeStyles.None,
                                                               out DateTime serverDate);
            if (isParsedSuccessfully)
            {
                validStartupTimeStamp = CalculateTimestamp(serverDate);
            }
            else
            {
                Debug.Log($"Date prase error from response: {response}");
            }
        }


        private void ReachabilityHandler_NetworkReachabilityStatusChanged(NetworkStatus status)
        {
            ReachabilityHandler.Instance.NetworkReachabilityStatusChanged -= ReachabilityHandler_NetworkReachabilityStatusChanged;

            Validate();
        }


        private void BackgroundService_OnApplicationSuspend(bool isEnteredBackground, TimeSpan timeSpan)
        {
            if (isEnteredBackground)
            {
                LastBackgroundEnterRealUtcTime = ValidUtcNow;
            }
        }


        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            bool shouldForceSaveTime = type == LogType.Exception || type == LogType.Error;

            if (shouldForceSaveTime)
            {
                LastBackgroundEnterRealUtcTime = ValidUtcNow;
                CustomPlayerPrefs.Save();
            }
        }

        #endregion
    }
}
