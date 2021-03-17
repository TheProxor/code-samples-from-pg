using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class BonusLevelShooterInputHelper : ILevelController
    {
        #region Fields

        private readonly BonusLevelController bonusLevelController;

        private bool canAim;
        private bool? wasCanceled;
        
        #endregion
        
        
        
        #region Properties
        
        public bool IsComponentEnabled { get; private set; }

        public bool IsAimingAvailable =>
            !IsComponentEnabled || canAim;
        
        #endregion
        
        
        
        
        #region Ctor

        public BonusLevelShooterInputHelper(BonusLevelController _bonusLevelController)
        {
            bonusLevelController = _bonusLevelController;
        }

        #endregion
        
        
        
        #region ILevelController
        
        public void Initialize()
        {
            IsComponentEnabled = GameServices.Instance.LevelEnvironment.Context.IsBonusLevel;
            canAim = false;

            if (IsComponentEnabled)
            {
                bonusLevelController.OnDecelerationBegin += BonusLevelController_OnDecelerationBegin;
                bonusLevelController.OnStageEnded += BonusLevelController_OnStageEnded;
                
                ShootersInputLevelController.OnDrawFinish += ShootersInputLevelController_OnDrawFinish;
                ShooterInput.OnDrawingCanceled += ShooterInput_OnDrawingCanceled;
            }
        }

        


        public void Deinitialize()
        {
            if (IsComponentEnabled)
            {
                bonusLevelController.OnDecelerationBegin -= BonusLevelController_OnDecelerationBegin;
                bonusLevelController.OnStageEnded -= BonusLevelController_OnStageEnded;
                
                ShootersInputLevelController.OnDrawFinish -= ShootersInputLevelController_OnDrawFinish;
                ShooterInput.OnDrawingCanceled -= ShooterInput_OnDrawingCanceled;
            }
        }
        
        #endregion
        
        
        
        #region Events handlers
        
        private void BonusLevelController_OnDecelerationBegin(int stageIndex)
        {
            canAim = true;
        }
        
        
        private void ShootersInputLevelController_OnDrawFinish(Shooter shooter, bool success, Vector2 touchPosition)
        {
            bool correct = wasCanceled == null ||
                           !wasCanceled.Value;
            
            if (success && correct)
            {
                canAim = false;
            }
            
            wasCanceled = null;
        }
        
        
        private void BonusLevelController_OnStageEnded(int stageIndex)
        {
            canAim = false;
        }
        
        
        private void ShooterInput_OnDrawingCanceled()
        {
            wasCanceled = true;
        }
        
        #endregion
        
    }
}

