using System;
using Drawmasters.Levels.Data;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class SkipLevelFinisher : ILevelFinisher
    {
        public void FinishLevel(Action onFinished)
        {
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            GameMode mode = context.Mode;

            PlayerData playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            onFinished();

            int index = playerData.GetModeCurrentIndex(mode);

            LevelsManager.Instance.LoadLevel(mode, index);
            LevelsManager.Instance.PlayLevel();
        }
    }
}