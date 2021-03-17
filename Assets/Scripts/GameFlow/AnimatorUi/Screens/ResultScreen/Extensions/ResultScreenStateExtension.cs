using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Ui
{
    public static class ResultScreenStateExtension
    {
        #region Properties
        
        private static bool CanClaimVideoSkin
        {
            get
            {
                bool result = true;

                result &= GameServices.Instance.ProposalService.SkinProposal.CanClaimSkin;
                result &= GameServices.Instance.LevelEnvironment.Progress.WasBarFillingShown;
                
                return result;
            }
        }
        
        #endregion
        
        
        
        #region IResultState

        public static ResultScreenState DefineState(this ResultScreenState thisState, LevelProgress levelProgress, LevelContext levelContext)
        {
            ResultScreenState definedState = ResultScreenState.None;

            if (levelProgress == null ||
                levelContext == null)
            {
                return ResultScreenState.Lose;
            }

            bool isLevelCompleted = levelProgress.LevelResultState.IsLevelOrProposalAccomplished();

            bool isLevelLost = levelProgress.LevelResultState == LevelResult.Lose;
            bool isLevelPaused = LevelsManager.Instance.Level.CurrentState == LevelState.Paused;
            bool isUsualLevelFlow = !levelContext.Mode.IsHitmastersLiveOps();

            if (levelProgress.LevelResultState == LevelResult.ProposalEnd &&
                levelContext.Mode.IsHitmastersLiveOps())
            {
                definedState = ResultScreenState.LiveopsProposalEnd;
            }
            else if (CanClaimVideoSkin && isUsualLevelFlow)
            {
                definedState = ResultScreenState.CanClaimSkin;
            }
            else if (isLevelCompleted)
            {
                bool isBonusLevelCompleted = levelContext.IsBonusLevel;
                bool isBossLevelCompleted = levelContext.IsBossLevel;

                if (isBonusLevelCompleted)
                {
                    definedState = ResultScreenState.BonusComplete;
                }
                else if (isBossLevelCompleted)
                {
                    definedState = ResultScreenState.BossComplete;
                }
                else if (levelContext.Mode.IsHitmastersLiveOps())
                {
                    definedState = ResultScreenState.LiveopsComplete;
                }
                else
                {
                    definedState = ResultScreenState.LevelComplete;
                }
            }            
            else if (isLevelLost)
            {
                definedState = ResultScreenState.Lose;
            }
            else if (isLevelPaused)
            {
                definedState = ResultScreenState.Paused;
            }
            else
            {
                CustomDebug.Log("Incorrect result state definition.");
                
                definedState = ResultScreenState.Lose;
            }

            return definedState;
        }

        #endregion
    }
}

