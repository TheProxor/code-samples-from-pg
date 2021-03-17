using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Modules.General;
using System;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class LevelTargetRocketTrajectoryComponent : LevelTargetComponent
    {
        #region Fields

        private readonly Dictionary<RocketLaunchData.Data, ShooterAimingDrawer> rocketDrawers;
        private readonly RocketLaunchDrawSettings settings;

        private RocketLaunchData[] stagesData;

        private object drawTrajectoriesGuid;

        #endregion



        #region Class lifecycle

        public LevelTargetRocketTrajectoryComponent()
        {
            rocketDrawers = new Dictionary<RocketLaunchData.Data, ShooterAimingDrawer>();

            settings = IngameData.Settings.rocketLaunchDrawSettings;
            drawTrajectoriesGuid = Guid.NewGuid();
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            if (levelTarget is EnemyBoss boss)
            {
                stagesData = boss.RocketLaunchData;
                StageLevelTargetComponent.OnShouldChangeStage += StageLevelTargetComponent_OnShouldChangeStage;
                boss.OnAppeared += EnemyBoss_OnAppeared;
                Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            }
            else
            {
                CustomDebug.Log($"No impleted logic for simple level target. Can't detect stages and appear callback in {this}");
                return;
            }
        }


        public override void Disable()
        {
            ClearTrajectories();
           
            StageLevelTargetComponent.OnShouldChangeStage -= StageLevelTargetComponent_OnShouldChangeStage;

            if (levelTarget is EnemyBoss boss)
            {
                boss.OnAppeared -= EnemyBoss_OnAppeared;
            }

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            DOTween.Kill(drawTrajectoriesGuid);
            
            TouchManager.Instance.IsEnabled = true;
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        private void ClearTrajectories()
        {
            foreach (var drawer in rocketDrawers)
            {
                drawer.Value.ClearDraw(true);
                drawer.Value.Deinitialize();
            }

            DOTween.Kill(this);
            rocketDrawers.Clear();
        }

        #endregion



        #region Events handlers

        private void StageLevelTargetComponent_OnShouldChangeStage(int stage, LevelTarget levelTarget)
        {
            ClearTrajectories();
        }


        private void EnemyBoss_OnAppeared()
        {
            bool isDataExists = LevelStageController.CurrentStageIndex < stagesData.Length;
            RocketLaunchData.Data[] currentStageData = isDataExists ? stagesData[LevelStageController.CurrentStageIndex].data : Array.Empty<RocketLaunchData.Data>();

            foreach (var data in currentStageData)
            {
                if (data.trajectory.Length != 0)
                {
                    ShooterAimingDrawer drawer = Content.Management.CreateRocketLaunchAimDrawer(data.colorType);
                    rocketDrawers.Add(data, drawer);
                }
            }

            foreach (var drawer in rocketDrawers)
            {
                drawer.Value.Initialize(null, GameServices.Instance.LevelEnvironment.Context.WeaponType);
                drawer.Value.ClearDraw(true);

                drawer.Value.StartDrawing(drawer.Key.trajectory[0]);
            }

            // immitate finger
            foreach (var drawer in rocketDrawers)
            {
                Sequence moveSequence = DOTween.Sequence();
                ShooterAimingDrawer savedDrawer = drawer.Value;
                Vector3[] trajectory = drawer.Key.trajectory;

                for (int i = 0; i < trajectory.Length - 1; i++)
                {
                    Vector3 startValue = trajectory[i];
                    Vector3 endValue = trajectory[i + 1];

                    float duration = Vector3.Distance(startValue, endValue) / settings.fullTrajectoryDrawSpeed;

                    moveSequence.Append(DOTween
                        .To(() => startValue, (value) =>
                    {
                        savedDrawer.DrawShotDirection(trajectory[0], value);
                    }, endValue, duration)
                        .SetId(drawTrajectoriesGuid));
                }

                float delay = LevelStageController.CurrentStageIndex == 0 ?
                    IngameData.Settings.bossLevelTargetSettings.drawTrajectoriesFirstDelay : IngameData.Settings.bossLevelTargetSettings.drawTrajectoriesDelay;

                moveSequence
                    .SetId(drawTrajectoriesGuid)
                    .SetDelay(delay)
                    .OnComplete(() => savedDrawer.EndDrawShotDirection(trajectory.FirstObject(), trajectory.LastObject()));
            }
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.ReturnToInitial)
            {
                List<Tween> drawTweens = DOTween.TweensById(drawTrajectoriesGuid);

                if (drawTweens != null)
                {
                    foreach (var tween in drawTweens)
                    {
                        tween.timeScale *= IngameData.Settings.bossLevelTargetSettings.drawTrajectoriesTimescaleMultiply;
                    }
                }
            }
        }

        #endregion
    }
}

