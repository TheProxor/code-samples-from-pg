using System;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Levels
{
    public static class LevelEndHandlerExtension
    {
        public static ILevelEndHandler DefineEnder(this ILevelEndHandler thisFinisher, Level level, Action<LevelState> _onStateChangeCallback)
        {
            ILevelEndHandler levelEndHandler;

            ILevelEnvironment levelEnvironment = GameServices.Instance.LevelEnvironment;

            LevelContext context = levelEnvironment.Context;
            
            if (context.Mode.IsHitmastersLiveOps())
            {
                levelEndHandler = new LiveOpsCommonEndHandler(level, _onStateChangeCallback);
            }
            else if (context.IsBossLevel)
            {
                levelEndHandler = new BossLevelEndHandler(level, _onStateChangeCallback);
            }
            else if (context.IsBonusLevel)
            {
                levelEndHandler = new BonusLevelEndHandler(GameServices.Instance.LevelControllerService.BonusLevelController);
            }
            else
            {
                levelEndHandler = new CommonLevelEndHandler(level, _onStateChangeCallback);
            }

            return levelEndHandler;
        }
    }
}
