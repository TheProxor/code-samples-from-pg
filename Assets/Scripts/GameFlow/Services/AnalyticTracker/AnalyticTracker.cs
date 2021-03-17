using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Analytics;


namespace Drawmasters.ServiceUtil
{
    public class AnalyticTracker : IAnalyticTracker
    {
        #region Fields
        
        private readonly ILevelEnvironment levelEnvironment;
        private readonly ITimeServices timeService;

        #endregion
        
        

        #region Ctor

        public AnalyticTracker(IProposalService proposalService,
                               ILevelEnvironment _levelEnvironment,
                               ITimeServices _timeService)
        {
            levelEnvironment = _levelEnvironment;
            timeService = _timeService;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region Analytic
        
        private void SendLevelFinishEvent()
        {
            LevelResult levelResult = levelEnvironment.Progress.LevelResultState;

            LevelContext context = levelEnvironment.Context;

            string levelId = context.Mode.IsDrawingMode()
                ? context.LevelId
                : $"{AnalyticsKeys.LiveOpsParameters.HitmastersLevelIdPrefix}{context.LevelId}";

            int levelIndex = context.LevelGlobalIndex + 1;

            CommonEvents.SendLevelFinish(levelIndex,
                timeService.LevelTimeInSeconds,
                levelResult.ConvertForAnalytic(),
                levelId);

            CommonEvents.SendSubLevelFinish(levelIndex,
                context.SublevelIndex + 1,
                timeService.LevelTimeInSeconds,
                levelResult.ConvertForAnalytic(),
                levelId);
        }

        
        private void SendLevelStartEvent()
        {
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;
            
            string levelId = context.Mode.IsDrawingMode()
                ? context.LevelId
                : $"{AnalyticsKeys.LiveOpsParameters.HitmastersLevelIdPrefix}{context.LevelId}";
                        
            int levelIndex = context.LevelGlobalIndex + 1;

            CommonEvents.SendLevelStart(levelIndex,
                levelId);    

            CommonEvents.SendSubLevelStart(levelIndex,
                context.SublevelIndex + 1,
                levelId);
        }
        
        #endregion


        
        #region Events handlers
        
        private void Level_OnLevelStateChanged(LevelState currentState)
        {
            //HACK
            if (currentState == LevelState.Unloaded ||
                GameServices.Instance.LevelEnvironment.Context.SceneMode.IsSceneMode())
            {
                return;
            }

            if (currentState == LevelState.Finished)
            {
                SendLevelFinishEvent();
            }
            else if (currentState == LevelState.Initialized)
            {
                SendLevelStartEvent();
            }
        }
        
        #endregion
    }
}