using System;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class ReloadLevelFinisher : ILevelFinisher
    {
        #region ILevelFinisher

        public void FinishLevel(Action onFinished)
        {
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            int index = context.Index;
            GameMode mode = context.Mode;

            onFinished?.Invoke();

            LevelsManager.Instance.LoadLevel(mode, index);
            LevelsManager.Instance.ReloadLevel();
        }

        #endregion
    }
}
