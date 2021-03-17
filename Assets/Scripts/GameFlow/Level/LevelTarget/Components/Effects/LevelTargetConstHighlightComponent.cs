using System;
using DG.Tweening;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelTargetConstHighlightComponent : LevelTargetComponent
    {
        #region Fields

        private Coroutine resetOutlineColorRoutine;

        private object outlineAnimationGuid;
        private LevelTargetSkinsSettings settings;

        #endregion



        #region Methods

        public override void Enable()
        {
            settings = IngameData.Settings.levelTargetSkinsSettings;
            outlineAnimationGuid = Guid.NewGuid();

            SetOutlineMaterial();
            PlayOutlineAnimation();

            if (levelTarget is EnemyBoss boss)
            {
                boss.OnStartCome += Boss_OnStartCome;
            }

            GameServices.Instance.LevelControllerService.Stage.OnStartChangeStage += Stage_OnStartChangeStage;
        }


        public override void Disable()
        {
            if (levelTarget is EnemyBoss boss)
            {
                boss.OnStartCome -= Boss_OnStartCome;
            }

            GameServices.Instance.LevelControllerService.Stage.OnStartChangeStage -= Stage_OnStartChangeStage;

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

            Color outlineColor = (levelTarget is EnemyBoss boss) ? settings.FindBossOutlineColor(boss.CurrentStageColorType) : default;
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

        #endregion



        #region Events handlers

        private void Boss_OnStartCome(float duration) =>
            PlayOutlineAnimation();
        

        private void Stage_OnStartChangeStage()
        {
            if (LevelStageController.CurrentStageIndex == 0)
            {
                PlayOutlineAnimation();
            }
        }

        #endregion
    }
}
