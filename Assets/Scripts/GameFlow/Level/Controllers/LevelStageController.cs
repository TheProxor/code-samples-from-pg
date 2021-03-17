using UnityEngine;
using System.Collections.Generic;
using Drawmasters.LevelsRepository;
using System;
using Modules.General;
using Drawmasters.Levels.Inerfaces;
using System.Linq;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class LevelStageController : ILevelController, IInitialStateReturn
    {
        #region Fields

        public event Action OnStartChangeStage;
        public event Action OnFinishChangeStage;

        public static event Action<LevelObjectData[]> OnShouldCreateObjects;

        private List<LevelObject> levelObjects;
        private Level.Data levelData;
        private LevelHeader levelHeader;
        private bool isBossLevel;
        private GameMode gameMode;

        #endregion



        #region Properties

        public static int StagesCount { get; private set; }

        public static int CurrentStageIndex { get; private set; }

        #endregion



        #region ILevelController

        public void Initialize()
        {
            isBossLevel = GameServices.Instance.LevelEnvironment.Context.IsBossLevel;
            gameMode = GameServices.Instance.LevelEnvironment.Context.Mode;

            if (!isBossLevel)
            {
                return;
            }

            CurrentStageIndex = 0;

            StageLevelTargetComponent.OnShouldChangeStage += StageLevelTargetComponent_OnShouldChangeState;
        }


        public void Deinitialize()
        {
            StageLevelTargetComponent.OnShouldChangeStage -= StageLevelTargetComponent_OnShouldChangeState;
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            levelData = null;
        }

        #endregion



        #region IInitialStateReturn

        public void ReturnToInitialState()
        {
            if (!isBossLevel)
            {
                return;
            }

            ReturnToStage(CurrentStageIndex);
        }

        #endregion



        #region Public methods

        public void SetupLevelObjects(List<LevelObject> _levelObjects) 
            => levelObjects = _levelObjects;


        public void SetupLevelData(Level.Data _levelData) 
            => levelData = _levelData;


        public void SetupLevelHeader(LevelHeader _levelHeader)
        {
            levelHeader = _levelHeader;
            StagesCount = levelHeader.stagesCount;
        }


        public void ApplyStage(int stage)
        {
            if (levelObjects == null ||
                levelData == null)
            {
                CustomDebug.Log($"Data not set for {nameof(LevelStageController)}");
                return;
            }

            CurrentStageIndex = stage;

            bool isImmediately = (stage == 0);
            float maxChangeDuration = IngameData.Settings.bossLevelSettings.delayBetweenStages;

            bool wasAnyObjectFreeFall = default;

            foreach (var levelObject in levelObjects)
            {
                LevelObjectData equalData = levelData.levelObjectsData.Find(e => levelObject.EqualData(e));

                if (equalData == null ||
                    equalData.stageData == null ||
                    equalData.stageData.Count <= stage)
                {
                    Debug.Log($"equalData is incorrect for {levelObject.name}");
                    continue;
                }

                StageLevelObjectData stageLevelObjectData = equalData.stageData[stage];

                if (stage < equalData.createdStageIndex)
                {
                    levelObject.PrepareForAppear(stageLevelObjectData);
                }
                else if (stageLevelObjectData.isFreeFall)
                {
                    levelObject.FreeFallObject(false);
                    wasAnyObjectFreeFall = true;
                }
                else if (levelObject.IsPrepareForCome)
                {
                    float objectComeDuration = levelObject.ComeDuration(stageLevelObjectData.position, stageLevelObjectData.comeVelocity) + IngameData.Settings.bossLevelSettings.objectsComeDelay;
                    maxChangeDuration = Mathf.Max(maxChangeDuration, objectComeDuration);

                    levelObject.ComeObject(stageLevelObjectData, isImmediately);
                }

                levelObject.StartStageChange(stageLevelObjectData, stage);
            }

            #warning Temp solution with additional delay for force shot. Vladislav.k
            float inputDisableDuration = maxChangeDuration + IngameData.Settings.bossLevelSettings.additionalShooterAllowShootDelay;
            Scheduler.Instance.CallMethodWithDelay(this, () => TouchManager.Instance.IsEnabled = true, inputDisableDuration);

            maxChangeDuration = isImmediately ? 0 : maxChangeDuration;

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                Physics2D.SyncTransforms();

                VisitAllLevelObjects((obj) =>
                {
                    if (gameMode.IsHitmastersLiveOps() && obj is Shooter shooter)
                    {
                        shooter.SetWeaponProjectiles(levelHeader.stageProjectilesCount[stage]);
                    }

                    obj.FinishStageChange(stage);
                });

                OnFinishChangeStage?.Invoke();
            }, maxChangeDuration);

            if (wasAnyObjectFreeFall)
            {
                PlayObjectsFallShake();
            }

            TouchManager.Instance.IsEnabled = false;

            OnStartChangeStage?.Invoke();

            void PlayObjectsFallShake()
            {
                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    CameraShakeSettings.Shake shake = IngameData.Settings.cameraShakeSettings.objectsFreeFall;
                    IngameCamera.Instance.Shake(shake);
                }, IngameData.Settings.bossLevelSettings.objectFreeFallDelay);
            }
        }

        #endregion



        #region Private methods

        private void ReturnToStage(int stage)
        {
            LevelObjectData[] objectsDataToRespawn = levelData.levelObjectsData
                .Where(e => levelObjects.Find(o => o.EqualData(e)) == null)
                .ToArray(); // find all destroyed

            objectsDataToRespawn = objectsDataToRespawn
                .Where(d => d.stageData.FindIndex(e => e.isFreeFall) == -1 || d.stageData.FindIndex(e => e.isFreeFall) > stage)
                .ToArray(); // select only that should be exists on stage

            OnShouldCreateObjects?.Invoke(objectsDataToRespawn);

            VisitAllLevelObjects((levelObject) =>
            {
                LevelObjectData equalData = levelData.levelObjectsData.Find(e => levelObject.EqualData(e));
                StageLevelObjectData stageLevelObjectData = equalData.stageData[stage];

                if (stage < equalData.createdStageIndex)
                {
                    levelObject.PrepareForAppear(stageLevelObjectData);
                }
                else if (stageLevelObjectData.isFreeFall)
                {
                    levelObject.SetupMaxGameZonePositionY();
                }
                else if (levelObject.IsPrepareForCome)
                {
                    levelObject.ComeObject(stageLevelObjectData, true);
                }

                levelObject.ReturnToStage(stageLevelObjectData, stage);
            });

            Physics2D.SyncTransforms();
        }


        private void VisitAllLevelObjects(Action<LevelObject> callback)
        {
            if (levelObjects == null)
            {
                return;
            }

            foreach (var levelObject in levelObjects)
            {
                if (levelObject != null)
                {
                    callback?.Invoke(levelObject);
                }
            }
        }

        #endregion



        #region Events handlers

        private void StageLevelTargetComponent_OnShouldChangeState(int stage, LevelTarget levelTarget) =>
            ApplyStage(stage);
        
        #endregion
    }
}
