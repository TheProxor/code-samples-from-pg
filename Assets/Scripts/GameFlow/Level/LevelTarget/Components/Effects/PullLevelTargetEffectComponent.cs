using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class PullLevelTargetEffectComponent : LevelTargetComponent
    {
        #region Fields

        private string startEffectKey;
        private string effectKey;

        private EffectHandler pulledEffectHandler;

        #endregion



        #region Methods

        public override void Enable()
        {
            ProjectilePullThrowComponent.OnObjectPull += ProjectilePullThrowComponent_OnObjectPull;
            ProjectilePullThrowComponent.OnObjectReleased += ProjectilePullThrowComponent_OnObjectReleased;

            // TODO content may be swap keys
            startEffectKey = EffectKeys.FxWeaponGraviGunCollisionEnemy;
            effectKey = EffectKeys.FxWeaponGraviGunAuraEnemy;
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
            if (levelTarget == pulledObject)
            {
                EffectManager.Instance.PlaySystemOnce(startEffectKey,
                                                      levelTarget.Ragdoll2D.EstimatedSkeletonPosition,
                                                      levelTarget.transform.rotation,
                                                      levelTarget.SkeletonAnimation.transform);
                StopPulledEffect();

                pulledEffectHandler = EffectManager.Instance.CreateSystem(effectKey,
                                                                          true,
                                                                          rigidbody2D.position,
                                                                          rigidbody2D.transform.rotation,
                                                                          rigidbody2D.transform);
                if (pulledEffectHandler != null)
                {
                    pulledEffectHandler.transform.localScale = Vector3.one;
                    pulledEffectHandler.Play();
                }
            }
        }


        private void ProjectilePullThrowComponent_OnObjectReleased(LevelObject releasedObject)
        {
            if (levelTarget == releasedObject)
            {
                StopPulledEffect();
            }
        }

        #endregion
    }
}
