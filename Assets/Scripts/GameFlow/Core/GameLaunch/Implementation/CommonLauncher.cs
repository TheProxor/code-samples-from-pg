using System;
using Drawmasters.Interfaces;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Modules.InAppPurchase;


namespace Drawmasters
{
    public class CommonLauncher : IGameLauncher
    {
        #region IGameLaunch

        public void Launch(Action onGameLaunched)
        {
            SubscriptionHandler handler = new SubscriptionHandler();
            handler.HandleStartSubscrption();

            IPlayerStatisticService playerStatistic = GameServices.Instance.PlayerStatisticService;
            
            LevelsManager.Instance.LoadScene(playerStatistic.PlayerData.LastPlayedMode,
                GameMode.MenuScene);
            
            UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, view => onGameLaunched?.Invoke());
        }

        #endregion
    }
}

