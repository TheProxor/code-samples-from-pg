using System;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Modules.Advertising;
using Modules.General;
using Modules.General.Abstraction;


namespace Drawmasters.Levels
{
    public class LoseLevelFinisher : ILevelFinisher
    {
        #region Fields

        protected Action callback;
        private AnimatorView resultScreen;
        private readonly ILevelEnvironment levelEnvironment;

        #endregion



        #region Ctor

        public LoseLevelFinisher()
        {
            levelEnvironment = GameServices.Instance.LevelEnvironment;
        }

        #endregion



        #region Methods

        public void FinishLevel(Action _onFinished)
        {
            callback = _onFinished;
            
            LevelContext context = levelEnvironment.Context;

            bool isLastSublevel = context.IsEndOfLevel;

            if (isLastSublevel)
            {
                AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial,
                    AdPlacementType.BeforeResult, result =>
                        Scheduler.Instance.CallMethodWithDelay(this, ShowLoseScreen, CommonUtility.OneFrameDelay));
            }
            else
            {
                ShowLoseScreen();
            }
        }


        private void ShowLoseScreen()
        {
            resultScreen = UiScreenManager.Instance.ShowScreen(ScreenType.Result);
            resultScreen.AddOnHiddenCallback(OnScreenHidden);
        }

        #endregion



        #region Events handlers

        private void OnScreenHidden(AnimatorView view)
        {
            if (resultScreen == view)
            {
                LevelProgress progress = levelEnvironment.Progress;

                if (progress.IsLevelSkipped)
                {
                    LevelProgressObserver.TriggerLevelStateChanged(LevelResult.ResultSkip);
                    ILevelFinisher finisher = default;
                    finisher = finisher.DefineFinisher();
                    finisher.FinishLevel(callback);
                    
                    // LevelsManager.Instance.CompleteLevel(LevelResult.ResultSkip);
                }
                else if (progress.IsLevelReloaded)
                {
                    // LevelsManager.Instance.CompleteLevel(LevelResult.Reload);
                    
                    LevelProgressObserver.TriggerLevelStateChanged(LevelResult.Reload);
                    ILevelFinisher finisher = default;
                    finisher = finisher.DefineFinisher();
                    finisher.FinishLevel(callback);
                }
            }
        }

        #endregion
    }
}