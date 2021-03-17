using System;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface ICommonStatisticsService
    {
        Action OnLevelUp { get; set; }

        bool WasStartSubscrptionShowed { get; }

        int SessionCount { get; }

        int MatchesCount { get; }

        int TodayMatchesCount { get; }

        int UniqueLevelsFinishedCount { get; }

        DateTime FirstLaunchDateTime { get; }

        int LevelsFinishedCount { get; }
        
        int GetLevelsFinishedCount(GameMode mode);

        void SetLevelsFinishedCount(GameMode mode, int value);

        void ResetLevelsFinishedCount(GameMode mode);

        bool IsFirstLaunch { get; }

        void MarkStartSubscrptionShowed();
        
        bool IsIapsAvailable { get; }
    }
}

