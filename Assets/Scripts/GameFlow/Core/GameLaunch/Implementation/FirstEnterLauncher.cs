using System;
using Drawmasters.Interfaces;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Modules.InAppPurchase;


namespace Drawmasters
{
    public class FirstEnterLauncher : IGameLauncher
    {
        #region Fields

        public static event Action OnShouldPauseAnalyticsTimer;
        public static event Action OnShouldResumeAnalyticsTimer;

        #endregion



        #region IGameLauncher

        public void Launch(Action onGameLaunched)
        {
            GameMode mode = IngameData.Settings.modesInfo.firstModeToPlay;
            int levelIndex = GameServices.Instance.PlayerStatisticService.PlayerData.GetModeCurrentIndex(mode);

            LevelsManager.Instance.LoadLevel(mode, levelIndex);
            LevelsManager.Instance.PlayLevel();

            OnShouldPauseAnalyticsTimer?.Invoke();

            SubscriptionHandler handler = new SubscriptionHandler();
            handler.HandleStartSubscrption(() => OnShouldResumeAnalyticsTimer?.Invoke());


            onGameLaunched?.Invoke();
        }

        #endregion
    }
}
