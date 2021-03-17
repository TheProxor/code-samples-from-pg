using Spine.Unity;
using DG.Tweening;
using System;
using Drawmasters.ServiceUtil;
using Modules.General;
using Spine;


namespace Drawmasters.Levels
{
    public class LimbsLaserLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<bool> OnShouldSetLaserSoundEnabled;

        private bool wasDestroyStarted;
        private bool allowBossHits;

        #endregion



        #region Methods

        public override void Enable()
        {
            wasDestroyStarted = false;
            levelTarget.OnShouldStartLaserDestroy += StartLaserDestroy;

            allowBossHits = true;
            StageLevelTargetComponent.OnShouldChangeStage += StageLevelTargetComponent_OnShouldChangeState;
            ProjectileLaserOnExplodeComponent.OnShouldStartLaserDestroy += ProjectileLaserOnExplodeComponent_OnShouldStartLaserDestroy;
        }


        public override void Disable()
        {
            levelTarget.OnShouldStartLaserDestroy -= StartLaserDestroy;
            StageLevelTargetComponent.OnShouldChangeStage -= StageLevelTargetComponent_OnShouldChangeState;
            ProjectileLaserOnExplodeComponent.OnShouldStartLaserDestroy -= ProjectileLaserOnExplodeComponent_OnShouldStartLaserDestroy;

            DOTween.Complete(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        private void StartLaserDestroy() =>
            StartLaserDestroy(false);


        private void StartLaserDestroy(bool shouldDestroyAllLimbs)
        {
            if (levelTarget.Type == LevelTargetType.Boss && !allowBossHits)
            {
                return;
            }

            if (levelTarget.Type == LevelTargetType.Boss
                && !levelTarget.AllowVisualizeDamage)
            {
                PerfectsManager.PerfectReceiveNotify(PerfectType.LaserHit, levelTarget.transform.position, levelTarget);
                levelTarget.MarkHitted();
                return;
            }

            if (!wasDestroyStarted)
            {
                PerfectsManager.PerfectReceiveNotify(PerfectType.LaserHit, levelTarget.transform.position, levelTarget);

                OnShouldSetLaserSoundEnabled?.Invoke(true);

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    OnShouldSetLaserSoundEnabled?.Invoke(false);

                    levelTarget.MarkHitted();
                    levelTarget.FinishGame();
                }, IngameData.Settings.levelTarget.laserDestroyDuration);

                foreach (var limb in levelTarget.Limbs)
                {
                    if (shouldDestroyAllLimbs || !levelTarget.IsChoppedOffLimb(limb.RootBoneName))
                    {
                        limb.StartLaserDestroy();
                    }
                }

                wasDestroyStarted = true;
            }
        }

        #endregion



        #region Events handlers

        private void StageLevelTargetComponent_OnShouldChangeState(int stage, LevelTarget anotherLevelTarget)
        {
            if (anotherLevelTarget != levelTarget)
            {
                return;
            }

            float immortalityDuration = IngameData.Settings.bossLevelSettings.delayBetweenStages;
            allowBossHits = false;

            Scheduler.Instance.CallMethodWithDelay(this, () => allowBossHits = true, immortalityDuration);
        }

        private void ProjectileLaserOnExplodeComponent_OnShouldStartLaserDestroy(LevelTarget anotherLevelTarget, float delay)
        {
            if (anotherLevelTarget != levelTarget)
            {
                return;
            }

            Scheduler.Instance.CallMethodWithDelay(this, () => StartLaserDestroy(true), delay);
        }

        #endregion
    }
}
