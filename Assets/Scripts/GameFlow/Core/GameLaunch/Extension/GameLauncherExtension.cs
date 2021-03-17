using Drawmasters.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters
{
    public static class GameLauncherExtension
    {
        public static IGameLauncher Define(this IGameLauncher thisLauncher)
        {
            ICommonStatisticsService statistic = GameServices.Instance.CommonStatisticService;

            if (statistic.IsFirstLaunch)
            {
                return new FirstEnterLauncher();
            }
            else
            {
                return new CommonLauncher();
            }
        }
    }
}
