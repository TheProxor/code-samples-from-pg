using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class LevelTargetHighlightComponent : LevelTargetComponent
    {
        #region Fields

        private bool shouldApplyOutlineMaterialOnEnable;
        private ShooterColorType lastShooterColorType;
        private Coroutine resetOutlineColorRoutine;

        private object outlineAnimationGuid;
        private LevelTargetSkinsSettings settings;

        #endregion



        #region Methods

        public override void Enable()
        {
            bool isLiveOps = GameServices.Instance.LevelEnvironment.Context.Mode.IsHitmastersLiveOps();

            if (isLiveOps)
            {
                return;
            }

            settings = IngameData.Settings.levelTargetSkinsSettings;
            outlineAnimationGuid = Guid.NewGuid();

            if (shouldApplyOutlineMaterialOnEnable && ColorTypesSolutions.ShouldHighlightLevelTarget(levelTarget, lastShooterColorType))
            {
                SetOutlineMaterial();
                PlayOutlineAnimation();
            }
            else
            {
                SetOriginalMaterial();
                StopOutlineAnimation();
            }

            shouldApplyOutlineMaterialOnEnable = false;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            ShootersInputLevelController.OnStartDraw += ShootersInputLevelController_OnStartDraw;

            lastShooterColorType = default;
        }


        public override void Disable()
        {
            bool isLiveOps = GameServices.Instance.LevelEnvironment.Context.Mode.IsHitmastersLiveOps();

            if (isLiveOps)
            {
                return;
            }

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            ShootersInputLevelController.OnStartDraw -= ShootersInputLevelController_OnStartDraw;

            MonoBehaviourLifecycle.StopPlayingCorotine(resetOutlineColorRoutine);
            resetOutlineColorRoutine = null;
            SetOriginalMaterial();
            StopOutlineAnimation();
        }


        private void SetOutlineMaterial()
        {
            Material originalMaterial = settings.FindOriginalMaterial(levelTarget.SkeletonAnimation.skeletonDataAsset);
            Material outlineMaterial = settings.FindOutlineMaterial(levelTarget.SkeletonAnimation.skeletonDataAsset);


            if (levelTarget.SkeletonAnimation.CustomMaterialOverride.ContainsKey(originalMaterial))
            {
                CustomDebug.Log("Outline material already setted!");
                return;
            }

            levelTarget.SkeletonAnimation.CustomMaterialOverride.Add(originalMaterial, outlineMaterial);
        }


        private void PlayOutlineAnimation()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            //TODO dmitry ??
            // levelTarget.Renderer.GetPropertyBlock(block);

            Color outlineColor = settings.FindOutlineColor(levelTarget.ColorType);
            block.SetColor("_OutlineColor", outlineColor);

            settings.thresholdAnimation.Play((value) =>
            {
                block.SetFloat("_ThresholdEnd", value);
                levelTarget.Renderer.SetPropertyBlock(block);
            }, outlineAnimationGuid);
        }


        private void StopOutlineAnimation() =>
            DOTween.Kill(outlineAnimationGuid);


        private void SetOriginalMaterial()
        {
            Material originalMaterial = settings.FindOriginalMaterial(levelTarget.SkeletonAnimation.skeletonDataAsset);

            if (levelTarget.SkeletonAnimation.CustomMaterialOverride.ContainsKey(originalMaterial))
            {
                levelTarget.SkeletonAnimation.CustomMaterialOverride.Remove(originalMaterial);
            }
        }


        private IEnumerator ResetOutlineMaterialRoutine()
        {
            yield return new WaitForSeconds(settings.outlineResetOnShotDelay);

            SetOriginalMaterial();
            StopOutlineAnimation();
        }

        #endregion



        #region Events handlers

        private void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 touchPosition)
        {
            lastShooterColorType = shooter.ColorType;

            SetOriginalMaterial();

            if (shooter.ColorType == levelTarget.ColorType)
            {
                SetOutlineMaterial();
                PlayOutlineAnimation();
            }
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.FinishDrawing:
                    MonoBehaviourLifecycle.StopPlayingCorotine(resetOutlineColorRoutine);
                    resetOutlineColorRoutine = MonoBehaviourLifecycle.PlayCoroutine(ResetOutlineMaterialRoutine());
                    break;

                case LevelState.ReturnToInitial:
                    shouldApplyOutlineMaterialOnEnable = true;
                    break;
            }
        }

        #endregion
    }
}
