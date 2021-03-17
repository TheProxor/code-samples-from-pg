using System;
using Drawmasters.Advertising;
using Drawmasters.Ui;
using Modules.Advertising;
using Modules.General.Abstraction;


namespace Drawmasters.Levels
{
    public abstract class AnimatedLevelFinisher : ILevelFinisher
    {
        #region Fields

        private Action onFinished;

        #endregion



        #region ILevelFinisher

        public void FinishLevel(Action _onFinished)
        {
            onFinished = _onFinished;

            AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial,
                AdsInterstitialPlaceKeys.BetweenSublevels, result =>                    
                {
                    UiScreenManager.Instance.HideScreenImmediately(ScreenType.Result);
                    UiScreenManager.Instance.HideAll();

                    FromLevelToLevel.PlayTransition(() =>
                    {
                        onFinished?.Invoke();
                        LoadNextLevelAction();
                    });
                });
        }

        #endregion



        #region Abstraction

        protected abstract void LoadNextLevelAction();

        #endregion
    }
}

