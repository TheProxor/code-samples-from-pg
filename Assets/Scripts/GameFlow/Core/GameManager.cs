using Drawmasters.Levels;
using Drawmasters.Vibration;
using System;
using UnityEngine;
using Modules.Sound;
using System.Collections.Generic;
using Drawmasters.Interfaces;


namespace Drawmasters
{
    public partial class GameManager : SingletonMonoBehaviour<GameManager>, IInitializable
    {
        #region Fields

        public static event Action<bool> OnGamePausedChange;
        public static event Action OnGameLaunched;

        private readonly List<object> pauseSenders = new List<object>();

        private bool isGamePaused;
        private float timeScaleBeforePause;

        #endregion



        #region Properties

        public bool IsDataDeleted { get; private set; }


        public bool MayDataBeDeleted { get; set; }


        public bool IsGamePaused
        {
            get => isGamePaused;

            private set
            {
                if (isGamePaused != value)
                {
                    isGamePaused = value;

                    timeScaleBeforePause = isGamePaused ? Time.timeScale : timeScaleBeforePause;
                    Time.timeScale = isGamePaused ? 0.0f : timeScaleBeforePause;

                    OnGamePausedChange?.Invoke(isGamePaused);
                }
            }
        }

        #endregion



        #region Unity lifecycle 

        private void OnEnable()
        {
            LLPrivacyManager.OnPersonalDataDeletingDetect += LLPrivacyManager_OnPersonalDataDeletingDetect;
            ApplicationManager.OnApplicationStarted += ApplicationManager_OnApplicationStarted;
            UaMenu.V2.DevContent.OnActiveStateChanged += DevCanvas_OnDevContentEnable;
        }


        private void OnDisable()
        {
            LLPrivacyManager.OnPersonalDataDeletingDetect -= LLPrivacyManager_OnPersonalDataDeletingDetect;
            ApplicationManager.OnApplicationStarted -= ApplicationManager_OnApplicationStarted;
            UaMenu.V2.DevContent.OnActiveStateChanged -= DevCanvas_OnDevContentEnable;

            VibrationManager.Deinitialize();
            PerfectsManager.Deinitialize();
        }

        #endregion



        #region Methods

        public void Initialize()
        {
        }


        public void SetGamePaused(bool isPaused, object pauseSender)
        {
            if (isPaused)
            {
                if (!pauseSenders.Contains(pauseSender))
                {
                    pauseSenders.Add(pauseSender);
                }
            }
            else
            {
                pauseSenders.Remove(pauseSender);
            }

            IsGamePaused = (pauseSenders.Count != 0);
        }

        #endregion
        
        

        #region Private methods

        private void LaunchGame()
        {
            IGameLauncher launcher = null;
            launcher = launcher.Define();
            launcher.Launch(() => OnGameLaunched?.Invoke());
        }

        #endregion



        #region Events handlers

        private void ApplicationManager_OnApplicationStarted() =>
            LaunchGame();


        private void DevCanvas_OnDevContentEnable(bool enabled) =>
            SetGamePaused(enabled, this);


        private void LLPrivacyManager_OnPersonalDataDeletingDetect()
        {
            if (TouchManager.HasFoundInstance)
            {
                TouchManager.Instance.IsEnabled = false;
            }

            SoundManager.MuteAll(true);
            SetGamePaused(true, this);

            IsDataDeleted = true;
        }

        #endregion
    }
}
