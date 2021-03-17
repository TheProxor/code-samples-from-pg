using System;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class LeaveLevelFinisher : ILevelFinisher
    {
        public void FinishLevel(Action onFinished)
        {
            ILevelEnvironment levelEnvironment = GameServices.Instance.LevelEnvironment;
            GameMode mode = levelEnvironment.Context.Mode;

            onFinished?.Invoke();

            LevelsManager.Instance.LoadScene(mode, GameMode.MenuScene);
            UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu);
        }
    }
}