using Drawmasters.Effects;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class RewardCollectVisualComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private BonusLevelSettings settings;
        
        #endregion
        
        
        
        #region Overrided methods

        public override void Enable()
        {
            settings = IngameData.Settings.bonusLevelSettings;
            
            RewardCollectComponent.OnRewardDropped += RewardCollectComponent_OnRewardDropped;
        }


        public override void Disable()
        {
            RewardCollectComponent.OnRewardDropped -= RewardCollectComponent_OnRewardDropped;
        }

        #endregion
        
        
        
        #region Events handlers

        private void RewardCollectComponent_OnRewardDropped(PhysicalLevelObject from, BonusLevelObjectData bonusLevelObjectData)
        {
            if (sourceLevelObject == from)
            {
                string vfxName = string.Empty;

                switch (bonusLevelObjectData.rewardType)
                {
                    case Proposal.RewardType.Currency:
                        vfxName = settings.FindCurrencyDestroyFx(bonusLevelObjectData.currencyType);
                        break;

                    case Proposal.RewardType.PetSkin:
                        vfxName = settings.commonDestroyFx;
                        break;
                }
                           
                if (!string.IsNullOrEmpty(vfxName))
                {
                    Transform root = sourceLevelObject.transform;

                    EffectManager.Instance.PlaySystemOnce(vfxName,
                        root.position,
                        root.rotation);
                }
                
                SoundManager.Instance.PlaySound(SoundGroupKeys.RandomCoinClaimKey);
            }
        }
        
        #endregion
    }
}

