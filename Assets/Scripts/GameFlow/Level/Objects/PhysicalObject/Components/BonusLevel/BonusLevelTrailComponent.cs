using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class BonusLevelTrailComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] [Enum(typeof(EffectKeys))]
        private string vfxName = default;

        #endregion
        #region Properties

        private BonusLevelController Controller { get; set; }
        
        private BonusLevelObjectData BonusData { get; set; }
        
        private EffectHandler EffectHandler { get; set; }

        #endregion
        
        
        
        #region Abstract implementation
        
        public override void Enable()
        {
            Controller = GameServices.Instance.LevelControllerService.BonusLevelController;
            BonusData = sourceLevelObject.CurrentData.bonusData;
            
            Controller.OnStageBegun += Controller_OnStageBegun;
            Controller.OnDecelerationBegin += Controller_OnDecelerationBegin;
        }

        

        public override void Disable()
        {
            Controller.OnStageBegun -= Controller_OnStageBegun;
            Controller.OnDecelerationBegin -= Controller_OnDecelerationBegin;
        }
        
        #endregion
        
        
        
        #region Events handlers
        
        private void Controller_OnDecelerationBegin(int stageIndex)
        {
            if (BonusData.stageIndex == stageIndex)
            {
                if (EffectHandler != null)
                {
                    EffectHandler.Stop();
                }
            }
        }

        private void Controller_OnStageBegun(int stageIndex)
        {
            if (BonusData.stageIndex == stageIndex)
            {
                EffectHandler = EffectManager.Instance.CreateSystem(vfxName, true, sourceLevelObject.transform.position,
                    parent: sourceLevelObject.transform);
            }
        }
        
        #endregion
    }
}

