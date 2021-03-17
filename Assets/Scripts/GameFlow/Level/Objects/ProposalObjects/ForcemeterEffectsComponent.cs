using System.Collections.Generic;
using Modules.Sound;
using MoreMountains.NiceVibrations;
using Drawmasters.Effects;
using Drawmasters.LevelTargetObject;
using Drawmasters.Proposal;
using Drawmasters.Ui;
using Drawmasters.Vibration;
using Spine.Unity;
using UnityEngine;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Levels
{
    public class ForcemeterEffectsComponent : ForcemeterComponent
    {
        #region Fields

        private readonly IdleEffect idleEffect;
        private readonly IdleEffect idleLampsEffect;
        private readonly IdleEffect idleWiresEffect;
        private readonly IdleEffect idleSmokeEffect;

        private readonly List<(EffectHandler, BoneFollower)> lampsHandlers;

        private readonly ForceMeterUiSettings settings;

        private EffectHandler hammerTrailtHandler;

        #endregion



        #region Class lifecycle

        public ForcemeterEffectsComponent(IdleEffect _idleEffect,
                                          IdleEffect _idleLampsEffect,
                                          IdleEffect _idleWiresEffect,
                                          IdleEffect _idleSmokeEffect)
        {
            idleEffect = _idleEffect;
            idleLampsEffect = _idleLampsEffect;
            idleWiresEffect = _idleWiresEffect;
            idleSmokeEffect = _idleSmokeEffect;

            settings = IngameData.Settings.forceMeterUiSettings;

            lampsHandlers = new List<(EffectHandler, BoneFollower)>();
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            idleEffect.CreateAndPlayEffect();
            idleLampsEffect.CreateAndPlayEffect();
            CommonUtility.SetObjectActive(forcemeter.FadeSprite.gameObject, false);

            ForceMeterScreen.OnShouldPlayHitAnimation += ForceMeterScreen_OnShouldPlayHitAnimation;
            ForceMeterScreen.OnShouldPlayHitAnimation += StopIdleLampsEffect;
            ForcemeterAnimationComponent.OnProgressStartFill += ForcemeterAnimationComponent_OnProgressStartFill;
            ForcemeterAnimationComponent.OnProgressFinishFill += ForcemeterAnimationComponent_OnProgressFinishFill;

            ShooterForcemeterComponent.OnShouldEnableLightning += ShooterForcemeterComponent_OnShouldEnableLightning;
        }


        public override void Disable()
        {
            ForceMeterScreen.OnShouldPlayHitAnimation -= ForceMeterScreen_OnShouldPlayHitAnimation;
            ForceMeterScreen.OnShouldPlayHitAnimation -= StopIdleLampsEffect;
            ForcemeterAnimationComponent.OnProgressStartFill -= ForcemeterAnimationComponent_OnProgressStartFill;
            ForcemeterAnimationComponent.OnProgressFinishFill -= ForcemeterAnimationComponent_OnProgressFinishFill;

            ShooterForcemeterComponent.OnShouldEnableLightning -= ShooterForcemeterComponent_OnShouldEnableLightning;

            idleEffect.StopEffect();
            idleLampsEffect.StopEffect();
            idleWiresEffect.StopEffect();
            idleSmokeEffect.StopEffect();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            foreach (var handler in lampsHandlers)
            {
                if (!handler.Item1.InPool)
                {
                    EffectManager.Instance.PoolHelper.PushObject(handler.Item1);
                }

                Object.Destroy(handler.Item2.gameObject);
            }

            if (hammerTrailtHandler != null && !hammerTrailtHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(hammerTrailtHandler);
            }

            lampsHandlers.Clear();
        }

        #endregion



        #region Events handlers

        private void StopIdleLampsEffect(int iterationIndex)
        {
            ForceMeterScreen.OnShouldPlayHitAnimation -= StopIdleLampsEffect;

            idleLampsEffect.StopEffect();
        }

        private void ForceMeterScreen_OnShouldPlayHitAnimation(int iterationIndex)
        {
            ColorAnimation hitFadeAppearAnimation = settings.FindHitFadeAppearAnimation(iterationIndex);

            forcemeter.FadeSprite.color = hitFadeAppearAnimation.beginValue;
            CommonUtility.SetObjectActive(forcemeter.FadeSprite.gameObject, true);
            hitFadeAppearAnimation.Play((value) => forcemeter.FadeSprite.color = value, this);

            ColorAnimation hitFadeDisappearAnimation = settings.FindHitFadeDisappearAnimation(iterationIndex);
            hitFadeDisappearAnimation.Play((value) => forcemeter.FadeSprite.color = value, this,
                () => CommonUtility.SetObjectActive(forcemeter.FadeSprite.gameObject, false));

            ShooterSkinType shooterSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin;
            bool isMale = IngameData.Settings.shooterSkinsSettings.GetSkinGenderType(shooterSkinType) == LevelTargetGenderType.Male;
            string sfxKey = isMale ? settings.FindMaleSfxKey(iterationIndex) : settings.FindFemaleSfxKey(iterationIndex);
            SoundManager.Instance.PlaySound(sfxKey);
        }


        private void ForcemeterAnimationComponent_OnProgressStartFill(int iterationIndex)
        {
            string effectKey = settings.FindButtonEffectKey(iterationIndex);
            EffectManager.Instance.PlaySystemOnce(effectKey, settings.GetButtonFxWorldPosition(forcemeter.SkeletonAnimation),
                parent: forcemeter.transform, transformMode: TransformMode.World);

            ForceMeterUiSettings.LampsStageAnimationData[] lampsData = settings.FindForcemeterLampsData(iterationIndex);

            foreach (var data in lampsData)
            {
                ForceMeterUiSettings.LampsStageAnimationData savedData = data;

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    foreach (var bone in savedData.forcemeterLightBones)
                    {
                        GameObject go = new GameObject("LampFxRoot", typeof(BoneFollower));
                        go.transform.SetParent(forcemeter.transform);

                        BoneFollower boneFollower = go.AddComponent<BoneFollower>();
                        boneFollower.skeletonRenderer = forcemeter.SkeletonAnimation;
                        boneFollower.SetBone(bone);

                        EffectHandler handler = EffectManager.Instance.CreateSystem(settings.lightEffectKeyName, true,
                            parent: boneFollower.transform, shouldOverrideLoops: false, transformMode: TransformMode.Local);
                        lampsHandlers.Add((handler, boneFollower));
                    }
                }, savedData.enableDelay);
            }

            HapticTypes vibrationType = settings.FindHapticType(iterationIndex);
            VibrationManager.Play(vibrationType);
        }


        private void ShooterForcemeterComponent_OnShouldEnableLightning(Transform hammerRoot)
        {
            EffectManager.Instance.PlaySystemOnce(settings.lightningEffectKey, default,
                parent: hammerRoot, transformMode: TransformMode.Local);

            hammerTrailtHandler = EffectManager.Instance.CreateSystem(settings.hammerTrailKey, true,
                            parent: hammerRoot, shouldOverrideLoops: false, transformMode: TransformMode.Local);
            if (hammerTrailtHandler != null)
            {
                hammerTrailtHandler.Play();
            }
        }


        private void ForcemeterAnimationComponent_OnProgressFinishFill(int iterationIndex)
        {
            EffectManager.Instance.PlaySystemOnce(settings.FindRewardReachEffectKey(iterationIndex), settings.FindRewardElementWorldPosition(iterationIndex, forcemeter.SkeletonAnimation),
                        parent: forcemeter.transform, transformMode: TransformMode.World);

            if (iterationIndex == settings.lampFxDisableIterationIndex)
            {
                idleEffect.StopEffect();
                idleWiresEffect.CreateAndPlayEffect();

                foreach (var handler in lampsHandlers)
                {
                    EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIDynamometerLampExplode, parent: handler.Item2.transform, transformMode: TransformMode.Local);

                    if (!handler.Item1.InPool)
                    {
                        EffectManager.Instance.PoolHelper.PushObject(handler.Item1);
                    }

                    handler.Item1.Clear();
                }

                idleSmokeEffect.CreateAndPlayEffect();
            }
        }

        #endregion
    }
}
