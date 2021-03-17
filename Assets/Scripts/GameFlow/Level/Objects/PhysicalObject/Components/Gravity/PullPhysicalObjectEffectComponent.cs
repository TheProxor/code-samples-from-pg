using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class PullPhysicalObjectEffectComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private string startEffectKey;
        private string effectKey;

        private Vector3 effectsScale;
        private EffectHandler pulledEffectHandler;

        #endregion



        #region Methods

        public override void Enable()
        {
            ProjectilePullThrowComponent.OnObjectPull += ProjectilePullThrowComponent_OnObjectPull;
            ProjectilePullThrowComponent.OnObjectReleased += ProjectilePullThrowComponent_OnObjectReleased;

            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(sourceLevelObject.WeaponType);

            //TODO content
            startEffectKey = string.Empty; // IngameData.Settings.projectileSkinsSettings.GetPullStartEffect(weaponSkinType, sourceLevelObject.PhysicalData);
            effectKey = string.Empty; // IngameData.Settings.projectileSkinsSettings.GetPullEffect(weaponSkinType, sourceLevelObject.PhysicalData);
            effectsScale = Vector3.one;// IngameData.Settings.projectileSkinsSettings.GetPullEffectsScale(weaponSkinType, sourceLevelObject.PhysicalData);
        }


        public override void Disable()
        {
            ProjectilePullThrowComponent.OnObjectPull -= ProjectilePullThrowComponent_OnObjectPull;
            ProjectilePullThrowComponent.OnObjectReleased -= ProjectilePullThrowComponent_OnObjectReleased;

            StopPulledEffect();
        }


        private void StopPulledEffect()
        {
            if (pulledEffectHandler != null && !pulledEffectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(pulledEffectHandler);
            }

            pulledEffectHandler = null;
        }

        #endregion



        #region Events handlers

        private void ProjectilePullThrowComponent_OnObjectPull(LevelObject pulledObject, Rigidbody2D rigidbody2D)
        {
            if (sourceLevelObject == pulledObject)
            {
                EffectHandler startEffectHandler = EffectManager.Instance.PlaySystemOnce(startEffectKey,
                                                                                         sourceLevelObject.transform.position,
                                                                                         sourceLevelObject.transform.rotation,
                                                                                         sourceLevelObject.transform);

                if (startEffectHandler != null)
                {
                    startEffectHandler.transform.localScale = effectsScale;
                }

                StopPulledEffect();
                pulledEffectHandler = EffectManager.Instance.CreateSystem(effectKey,
                                                                          true,
                                                                          sourceLevelObject.transform.position,
                                                                          sourceLevelObject.transform.rotation,
                                                                          sourceLevelObject.transform);
                if (pulledEffectHandler != null)
                {
                    pulledEffectHandler.transform.localScale = effectsScale;
                    pulledEffectHandler.Play();
                }
            }
        }


        private void ProjectilePullThrowComponent_OnObjectReleased(LevelObject releasedObject)
        {
            if (sourceLevelObject == releasedObject)
            {
                StopPulledEffect();
            }
        }

        #endregion
    }
}
