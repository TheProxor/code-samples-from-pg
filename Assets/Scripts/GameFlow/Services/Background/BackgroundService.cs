using System;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General;


namespace Drawmasters.ServiceUtil
{
    public class BackgroundService : IBackgroundService
    {
        #region Properties

        private DateTime LastBackgroundStateChangeTime
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Application.LastBackgroundStateChangeTime, DateTime.Now);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Application.LastBackgroundStateChangeTime, value);
        }

        #endregion



        #region IBackgroundService

        public event Action<bool, TimeSpan> OnApplicationSuspend;

        #endregion



        #region Ctor

        public BackgroundService()
        {
            GameManager.OnGameLaunched += GameManager_OnGameLaunched;
            ApplicationManager.OnApplicationStarted += ApplicationManager_OnApplicationStarted;
        }



        #endregion




        #region Private methods

        private void CheckSuspendEvent(bool isEnteredBackground)
        {
            TimeSpan currentInterval = DateTime.Now - LastBackgroundStateChangeTime;

            if (currentInterval.TotalSeconds < 0.0f)
            {
                currentInterval = TimeSpan.FromSeconds(0.0f);
            }
                #if UNITY_ANDROID
                    if (!isEnteredBackground)
                    {
                        MonoBehaviourLifecycle.PlayCoroutine(CommonUtility.CallInEndOfFrame(() =>
                        {
                            OnApplicationSuspend?.Invoke(isEnteredBackground, currentInterval);
                        }));
                    }
                    else
                #endif
            {
                OnApplicationSuspend?.Invoke(isEnteredBackground, currentInterval);
            }

            LastBackgroundStateChangeTime = DateTime.Now;
        }

        #endregion


        #region Events handlers

        private void GameManager_OnGameLaunched()
        {
            GameManager.OnGameLaunched -= GameManager_OnGameLaunched;

            LLApplicationStateRegister.OnApplicationEnteredBackground += LLApplicationStateRegister_OnApplicationEnteredBackground;
        }


        private void LLApplicationStateRegister_OnApplicationEnteredBackground(bool isEnteredBackground)
        {
            CheckSuspendEvent(isEnteredBackground);
        }


        private void ApplicationManager_OnApplicationStarted()
        {
            CheckSuspendEvent(false);
        }

        #endregion
    }
}

