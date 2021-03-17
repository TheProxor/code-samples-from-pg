using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;


namespace Drawmasters.ServiceUtil
{
    public class PlayerStatisticService : IPlayerStatisticService
    {
        #region IPlayerStatisticService

        public PlayerModesData ModesData { get; private set; }

        public PlayerChaptersData ChaptersData { get; private set; }

        public PlayerCurrencyData CurrencyData { get; private set; }

        public PlayerData PlayerData { get; private set; }

        public PlayerMansionData PlayerMansionData { get; private set; }

        public PlayerLiveOpsLeagueData PlayerLiveOpsLeagueData { get; private set; }

        #endregion



        #region Class lifecycle

        public PlayerStatisticService(IAbTestService abTestService,
                                      ILevelEnvironment levelEnvironment,
                                      IBackgroundService backgroundService)
        {
            ModesData = new PlayerModesData();
            ChaptersData = new PlayerChaptersData(ModesData, levelEnvironment);

            CurrencyData = new PlayerCurrencyData(levelEnvironment);

            PlayerData = new PlayerData(levelEnvironment, backgroundService);
            PlayerMansionData = new PlayerMansionData();
            PlayerLiveOpsLeagueData = new PlayerLiveOpsLeagueData();
        }

        #endregion
    }
}
