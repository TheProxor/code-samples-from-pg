using System.Collections.Generic;

namespace Drawmasters.Levels.Helpers
{
    public static class LevelStateExtension
    {
        #region Fields

        private static readonly HashSet<LevelState> ExceptFinishStates = new HashSet<LevelState>
        {
            LevelState.None, 
            LevelState.EndPlaying,
            LevelState.Finished, 
            LevelState.Unloaded
        };

        #endregion
        
        
        
        #region Api
        
        public static bool CanSkipOnIngame(this LevelState thisState) =>
            thisState == LevelState.Playing;

        public static bool CanSkipOnResult(this LevelState thisState) =>
            thisState == LevelState.EndPlaying;
        

        public static bool CanReload(this LevelState thisState) =>
            thisState == LevelState.Playing;
        

        public static bool CanFinish(this LevelState thisLevelState, LevelResult resultState)
        {
            bool canFinish = true;

            bool isValidState = !ExceptFinishStates.Contains(thisLevelState);
            bool isValidIngameSkipState = resultState != LevelResult.IngameSkip || thisLevelState.CanSkipOnIngame();
            bool isValidResultSkipState = resultState != LevelResult.ResultSkip || thisLevelState.CanSkipOnResult();
            
            canFinish &= isValidState;
            canFinish &= isValidIngameSkipState;
            canFinish &= isValidResultSkipState;

            return canFinish;
        }
        
        #endregion
    }
}