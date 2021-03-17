using Drawmasters.Levels;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;


namespace Drawmasters.ServiceUtil
{
    public class TimeService : ITimeServices
    {
        #region Fields

        private readonly Timer levelTimer;

        private readonly StatisticsTimer statisticsTimer;

        #endregion

        
        #region ITimeServices
        
        public int LevelTimeInSeconds => (int)levelTimer.RoundedValue;

        #endregion



        #region Ctor

        public TimeService(IBackgroundService backgroundService)
        {
            levelTimer = new Timer();

            statisticsTimer = new StatisticsTimer(backgroundService);
            statisticsTimer.Initialize();
            
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            FirstEnterLauncher.OnShouldPauseAnalyticsTimer += FirstEnterLauncher_OnShouldPauseAnalyticsTimer;
            FirstEnterLauncher.OnShouldResumeAnalyticsTimer += FirstEnterLauncher_OnShouldResumeAnalyticsTimer;
        }
        
        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.Unloaded)
            {
                return;
            }
            if (GameServices.Instance.LevelEnvironment.Context.IsSceneMode)
            {
                return;
            }
            
            if (levelState == LevelState.Initialized)
            {
                levelTimer.Reset();
                levelTimer.Start();
            }
            else if (levelState == LevelState.Finished)
            {
                levelTimer.Stop();
            }
        }

        
        private void FirstEnterLauncher_OnShouldResumeAnalyticsTimer() =>
            levelTimer.Start();
        

        private void FirstEnterLauncher_OnShouldPauseAnalyticsTimer() =>
            levelTimer.Stop();


        #endregion
    }
}