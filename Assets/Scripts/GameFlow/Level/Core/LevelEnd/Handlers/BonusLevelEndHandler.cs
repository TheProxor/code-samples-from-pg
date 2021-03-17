using System;
using Drawmasters.ServiceUtil;
using Modules.General;

namespace Drawmasters.Levels
{
    public class BonusLevelEndHandler : ILevelEndHandler
    {
        #region Ctor

        private readonly BonusLevelController bonusLevelController;
        
        #endregion
        
        
        
        #region Ctor

        public BonusLevelEndHandler(BonusLevelController _bonusLevelController)
        {
            bonusLevelController = _bonusLevelController;
        }
        
        #endregion
        
        
        
        #region ILeveEndHandler

        public event Action<LevelResult> OnEnded;

        public void Initialize()
        {
            bonusLevelController.OnBonusLevelFinished += BonusLevelController_OnBonusLevelFinished;
        }


        public void Deinitialize()
        {
            bonusLevelController.OnBonusLevelFinished -= BonusLevelController_OnBonusLevelFinished;
            
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        
        #endregion
        
        
        
        #region Events handlers
        
        private void BonusLevelController_OnBonusLevelFinished()
        {
            float delay = IngameData.Settings.bonusLevelSettings.levelEndDelay;

            Scheduler.Instance.CallMethodWithDelay(this, () => OnEnded?.Invoke(LevelResult.Complete), delay);            
        }

        #endregion
    }
}

