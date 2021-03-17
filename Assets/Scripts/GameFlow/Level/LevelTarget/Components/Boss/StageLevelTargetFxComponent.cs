using UnityEngine;
using System.Collections.Generic;
using Drawmasters.Effects;
using Modules.General;
using Drawmasters.ServiceUtil;
using System.Collections;
using DG.Tweening;
using Modules.Sound;

namespace Drawmasters.Levels
{
    public class StageLevelTargetFxComponent : LevelTargetComponent
    {
        #region Fields

        private int fxHitsIterator;

        private readonly (Transform, string)[] fxHitData;
        private readonly string destroyFxKey;

        private readonly List<EffectHandler> damageEffectHandlers = new List<EffectHandler>();
        private readonly CameraShakeSettings shakeSettings;
        private readonly BossLevelTargetSettings bossSettings;

        private EffectHandler currentHandler;

        private LevelStageController stageController;
        private EnemyBossBase enemyBoss;

        private Coroutine defeatRoutine;

        #endregion



        #region Class lifecycle

        public StageLevelTargetFxComponent((Transform, string)[] _fxHitData, string _destroyFxKey)
        {
            fxHitData = _fxHitData;
            destroyFxKey = _destroyFxKey;

            shakeSettings = IngameData.Settings.cameraShakeSettings;
            bossSettings = IngameData.Settings.bossLevelTargetSettings;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            enemyBoss = levelTarget as EnemyBossBase;

            if (enemyBoss == null)
            {
                CustomDebug.Log("wrong level target type.");
                return;
            }

            GameServices.Instance.LevelControllerService.Stage.OnStartChangeStage += Stage_OnStartChangeStage;
            GameServices.Instance.LevelControllerService.Stage.OnFinishChangeStage += StageController_OnFinishChangeStage;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            levelTarget.OnDefeated += LevelTarget_OnDefeated;
            enemyBoss.SetLimbsEnabled(true);
        }


        public override void Disable()
        {
            TouchManager.Instance.IsEnabled = true;

            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            MonoBehaviourLifecycle.StopPlayingCorotine(defeatRoutine);

            DestroyDamageFxs();

            levelTarget.OnDefeated -= LevelTarget_OnDefeated;

            GameServices.Instance.LevelControllerService.Stage.OnStartChangeStage -= Stage_OnStartChangeStage;
            GameServices.Instance.LevelControllerService.Stage.OnFinishChangeStage -= StageController_OnFinishChangeStage;
            StageLevelTargetComponent.OnShouldChangeStage -= StageLevelTargetComponent_OnShouldChangeStage;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            levelTarget.OnHitted -= LevelTarget_OnHitted;
        }


        private void DestroyDamageFxs()
        {
            foreach (var handler in damageEffectHandlers)
            {
                if (handler != null && !handler.InPool)
                {
                    EffectManager.Instance.PoolHelper.PushObject(handler);
                }
            }

            damageEffectHandlers.Clear();
        }


        private void PlayBossShake(CameraShakeSettings.Shake shake, bool resetPreviousShake = true) =>
            IngameCamera.Instance.Shake(shake, resetPreviousShake);


        private IEnumerator DefeatFxRoutine()
        {
            yield return new WaitForSecondsRealtime(bossSettings.startDefeatDelay);

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.BOSSDEATH_EXPLOSION);

            VectorAnimation moveAnimation = bossSettings.defeatMoveToCenterAnimation;

            moveAnimation.SetupBeginValue(levelTarget.transform.position);
            Vector3 endPosition = levelTarget.transform.position + (Vector3.zero - levelTarget.transform.position).normalized * bossSettings.defeatMoveToCenterDistance;
            moveAnimation.SetupEndValue(endPosition);
            moveAnimation.Play((value) => levelTarget.transform.position = value, this);

            bossSettings.defeatColorAnimation.Play((value) => enemyBoss.SetSlotsColor(value), this);

            yield return new WaitForSecondsRealtime(bossSettings.explosionDelay - bossSettings.startDefeatDelay);

            EffectManager.Instance.PlaySystemOnce(destroyFxKey, 
                levelTarget.FocusPostion, 
                levelTarget.transform.rotation);

            yield return new WaitForSecondsRealtime(bossSettings.defeatDelay - bossSettings.explosionDelay);

            DestroyDamageFxs();
            PlayBossShake(shakeSettings.bossStageChange);

            levelTarget.FinishGame();
        }
        
        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                foreach (var shake in shakeSettings.bossFirstCome)
                {
                    PlayBossShake(shake, false);
                }
            }

            if (state == LevelState.ReturnToInitial ||
                state == LevelState.FinishDrawing ||
                state == LevelState.FriendlyDeath)
            {
                Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            }

            if (state == LevelState.ReturnToInitial)
            {
                levelTarget.OnHitted -= LevelTarget_OnHitted;
                levelTarget.OnHitted += LevelTarget_OnHitted;

                if (!currentHandler.IsNull() && !currentHandler.InPool)
                {
                    damageEffectHandlers.Remove(currentHandler);
                    EffectManager.Instance.PoolHelper.PushObject(currentHandler);
                }

                enemyBoss.SetLimbsEnabled(true);

                MonoBehaviourLifecycle.StopPlayingCorotine(defeatRoutine);
                DOTween.Kill(this);
            }
        }


        private void LevelTarget_OnDefeated(LevelTarget defeatedLevelTarget)
        {
            if (levelTarget == defeatedLevelTarget)
            {
                defeatRoutine = MonoBehaviourLifecycle.PlayCoroutine(DefeatFxRoutine());
            }
        }


        private void LevelTarget_OnHitted(LevelTarget hittedLevelTarget)
        {
            hittedLevelTarget.OnHitted -= LevelTarget_OnHitted;

            if (fxHitsIterator < fxHitData.Length)
            {
                (Transform, string) data = fxHitData[fxHitsIterator];
                currentHandler = EffectManager.Instance.CreateSystem(data.Item2, true, default, default, data.Item1, TransformMode.Local);
                damageEffectHandlers.Add(currentHandler);
            }

            PlayBossShake(shakeSettings.bossStageChange);
        }


        private void Stage_OnStartChangeStage()
        {
            if (LevelStageController.CurrentStageIndex != 0)
            {
                enemyBoss.SetLimbsEnabled(false);
            }
        }


        private void StageController_OnFinishChangeStage()
        {
            fxHitsIterator = LevelStageController.CurrentStageIndex;
            currentHandler = null;

            levelTarget.OnHitted += LevelTarget_OnHitted;
            enemyBoss.SetLimbsEnabled(true);
        }


        private void StageLevelTargetComponent_OnShouldChangeStage(int stage, LevelTarget levelTarget)
        {
            fxHitsIterator = stage - 1;
            currentHandler = null;

            levelTarget.OnHitted += LevelTarget_OnHitted;
        }

        #endregion
    }
}