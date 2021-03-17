using Drawmasters.Statistics.Data;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IPlayerStatisticService
    {
        PlayerModesData ModesData { get; }

        PlayerChaptersData ChaptersData { get; }

        PlayerCurrencyData CurrencyData { get; }

        PlayerData PlayerData { get; }

        PlayerMansionData PlayerMansionData { get; }

        PlayerLiveOpsLeagueData PlayerLiveOpsLeagueData { get; }
    }
}

