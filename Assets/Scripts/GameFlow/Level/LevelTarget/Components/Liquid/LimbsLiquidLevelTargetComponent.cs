using DG.Tweening;
using Drawmasters.Effects;
using System;
using System.Collections.Generic;
using Modules.General;


namespace Drawmasters.Levels
{
    public class LimbsLiquidLevelTargetComponent : LimbsCollisionHandler
    {
        #region Fields

        public static event Action<bool> OnShouldSetAcidSoundEnabled;

        private bool wasCorroseStarted;

        private readonly List<EffectHandler> effectsHandlers = new List<EffectHandler>();

        #endregion



        #region Lifecycle

        public LimbsLiquidLevelTargetComponent(List<LevelTargetLimb> _enemyLimbs) : base(_enemyLimbs) { }

        #endregion



        #region Methods

        public override void Enable()
        {
            base.Enable();

            wasCorroseStarted = false;
        }


        public override void Disable()
        {
            DOTween.Complete(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            foreach (var i in effectsHandlers)
            {
                if (i != null && !i.InPool)
                {
                    EffectManager.Instance.PoolHelper.PushObject(i);
                }
            }

            effectsHandlers.Clear();

            base.Disable();
        }


        protected override void HandleCollidableObjectCollision(CollidableObject collidableObject, LevelTargetLimb limb)
        {
            LiquidLevelObject liquid = collidableObject.LiquidLevelObject;

            if (collidableObject.LiquidLevelObject == null)
            {
                return;
            }

            if (!wasCorroseStarted &&
                !levelTarget.IsChoppedOffLimb(limb.RootBoneName))
            {
                OnShouldSetAcidSoundEnabled?.Invoke(true);

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    levelTarget.MarkHitted();

                    foreach (var l in levelTarget.Limbs)
                    {
                        if (!levelTarget.IsChoppedOffLimb(l.RootBoneName))
                        {
                            ExplosionUtility.ExplodeLimb(l.RootBoneName, levelTarget);
                        }
                    }

                    OnShouldSetAcidSoundEnabled?.Invoke(false);

                }, IngameData.Settings.levelTarget.corroseDuration);

                PlayLimbsEffects(levelTarget.Limbs);

                wasCorroseStarted = true;
            }
            else if (levelTarget.IsChoppedOffLimb(limb.RootBoneName))
            {
                OnShouldSetAcidSoundEnabled?.Invoke(true);

                PlayCorroseEffect(limb);

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    ExplosionUtility.ExplodeLimb(limb.RootBoneName, levelTarget);

                    OnShouldSetAcidSoundEnabled?.Invoke(false);

                }, IngameData.Settings.levelTarget.corroseDuration);
            }
        }


        private void PlayLimbsEffects(List<LevelTargetLimb> targetLimbs)
        {
            foreach (var levelTargetLimb in targetLimbs)
            {
                if (!levelTarget.IsChoppedOffLimb(levelTargetLimb.RootBoneName) &&
                    !levelTarget.ExplodedLimbs.Contains(levelTargetLimb.RootBoneName))
                {
                    PlayCorroseEffect(levelTargetLimb);
                }
            }
        }


        private void PlayCorroseEffect(LevelTargetLimb levelTargetLimb)
        {
            LevelTargetSettings.LimbData foundData = levelTarget.Settings.FindLimbData(levelTargetLimb.RootBoneName);

            if (foundData == null)
            {
                return;
            }

            EffectHandler handler = EffectManager.Instance.PlaySystemOnce(foundData.acidDropEffectKey,
                                                                          levelTargetLimb.transform.position,
                                                                          levelTargetLimb.transform.rotation,
                                                                          levelTargetLimb.transform);
            if (handler != null)
            {
                effectsHandlers.Add(handler);
            }
        }

        #endregion
    }
}
