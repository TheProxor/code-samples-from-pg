using System;
using System.Collections.Generic;
using Drawmasters.LevelsRepository;
using UnityEngine;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;
using Modules.General;


namespace Drawmasters.Levels
{
    public partial class Level : MonoBehaviour
    {
        #region Fields

        public const int DoubleSkinProgressLevelIndex = 8;

        public event Action<LevelResult> OnLevelEnd;

        public static event Action<LevelState> OnLevelStateChanged;

        [SerializeField] private Transform objectsRoot = default;

        //TODO temporary public
        public readonly List<LevelObject> levelObjects = new List<LevelObject>();

        //TODO temporary public
        public IHeaderLoader headerLoader;

        private ILevelEndHandler levelEndHandler;

        private ILevelEnvironment levelEnvironment;

        private ILevelControllerService levelControllerService;

        #endregion



        #region Properties

        public LevelState CurrentState { get; private set; } = LevelState.None;


        #endregion



        #region IInitializable

        public void Initialize()
        {
            levelEnvironment = GameServices.Instance.LevelEnvironment;
            levelControllerService = GameServices.Instance.LevelControllerService;
        }

        #endregion


        #region Methods

        public void LoadLevel(GameMode gameMode, 
            int levelIndex, bool isEditor, WeaponType weapon, GameMode sceneMode)
        {
            if (levelEnvironment.Context != null)
            {
                CustomDebug.Log("Error. Context must be unloaded");
            }

            ClearObjects(levelObjects);
            LoadData(gameMode, levelIndex, isEditor, weapon, sceneMode);

            levelControllerService.Initialize();
            levelControllerService.OnLevelStateChanged += LevelControllerService_OnLevelStateChanged;

            levelControllerService.Stage.OnStartChangeStage += LevelStageController_OnStartChangeStage;
            levelControllerService.Stage.OnFinishChangeStage += LevelStageController_OnFinishChangeStage;

            ChangeState(LevelState.Initialized);
        }
        

        public void PlayLevel(bool isReloaded = false)
        {
            ILevelLoader loader = default;            
            loader = loader.DefineLoader(() => ChangeState(LevelState.Tutorial), isReloaded);
            
            loader.LoadLevel(() =>
            {
                ChangeState(LevelState.Playing);
            });
        }


        public void ForcePlay()
        {
            ChangeState(LevelState.Playing);
        }


        public void FinishPlayLevel()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            ChangeState(LevelState.Finished);

            UnloadLevel();
        }


        private void LoadData(GameMode gameMode,
                              int levelIndex,
                              bool isEditor,
                              WeaponType weapon,
                              GameMode sceneMode)
        {
            //TODO little hack
            bool isPremiumProposalScene = false;
            if (sceneMode == GameMode.PremiumForcemeterScene)
            {
                sceneMode = GameMode.ForcemeterScene;
                isPremiumProposalScene = true;
            }
            else if (sceneMode == GameMode.PremiumRouletteScene)
            {
                sceneMode = GameMode.RouletteScene;
                isPremiumProposalScene = true;
            }
            
            
            headerLoader = LevelHeaderLoaderCreator.CreateLoader(gameMode, sceneMode, isEditor);
            headerLoader.LoadHeader(gameMode, levelIndex, sceneMode, weapon);

            Data levelData = LevelsContainer.GetLevelData(headerLoader.LoadedHeader);


            WeaponType loadedWeapon = default;

            if (sceneMode != GameMode.None)
            {
                GameModesInfo.TryConvertModeToWeapon(gameMode,
                                                     out loadedWeapon);
            }
            else
            {
                loadedWeapon = headerLoader.LoadedHeader.weaponType;                
            }

            ILevelEnvironment service = GameServices.Instance.LevelEnvironment;
            service.LoadEnvironment(gameMode,
                                    levelIndex,
                                    isEditor,
                                    loadedWeapon,
                                    sceneMode,
                                    headerLoader.LoadedHeader.levelType,
                                    levelData,
                                    headerLoader.LoadedHeader.projectilesCount,
                                    isPremiumProposalScene);

            CreateObjects(levelData.levelObjectsData);

            levelEndHandler = levelEndHandler.DefineEnder(this, ChangeState);
            levelEndHandler.Initialize();
            levelEndHandler.OnEnded += LevelEndHandler_OnEnded;

            Physics2D.SyncTransforms();
            Linker.SetLinks(levelObjects, levelData.linkerData);
            Physics2D.SyncTransforms();

            levelObjects.ForEach(levelObject => levelObject.StartGame(service.Context.Mode,
                                                                      service.Context.WeaponType,
                                                                      transform));

            if (headerLoader.LoadedHeader.levelType == LevelType.Boss)
            {
                levelControllerService.Stage.SetupLevelHeader(headerLoader.LoadedHeader);
                levelControllerService.Stage.SetupLevelObjects(levelObjects);
                levelControllerService.Stage.SetupLevelData(levelData);
                levelControllerService.Stage.ApplyStage(0);
            }
        }


        //TODO temporary public
        public LevelObject CreateObject(LevelObjectData levelObjectData)
        {
            var context = GameServices.Instance.LevelEnvironment.Context;

            LevelObject levelObject = Content.Management.CreateLevelObject(levelObjectData.index, objectsRoot);

            if (levelObject != null)
            {
                levelObject.PreSetData();
                levelObject.SetData(levelObjectData);

                if (levelObject is Shooter shooterObject)
                {
                    levelControllerService.ShootersInput.Add(shooterObject);
                    levelControllerService.LineInput.AddShooter(shooterObject);

                    shooterObject.Initialize(context.WeaponType, context.ProjectilesCount);
                }

                if (levelObject is LevelTarget levelTarget)
                {
                    levelControllerService.Target.Add(levelTarget);
                }
                else if (levelObject is PhysicalLevelObject physicalObject)
                {
                    levelControllerService.PhysicalObjects.Add(physicalObject);
                }
                else if (levelObject is CurrencyLevelObject currencyLevelObject)
                {
                    levelControllerService.CurrencyObjectsLevelController.Add(currencyLevelObject);
                }

                // HACK, think about it
                if (levelObject is LevelObjectMonolith levelObjectMonolith)
                {
                    levelControllerService.BonusLevelController.AddMonolith(levelObjectMonolith);
                }

                levelObject.OnGameFinished += LevelObject_OnGameFinished;

                levelObjects.Add(levelObject);
            }

            return levelObject;
        }


        private void CreateObjects(List<LevelObjectData> levelObjectsData)
        {
            foreach (var levelObjectInfo in levelObjectsData)
            {
                CreateObject(levelObjectInfo);
            }
        }


        public void UnloadLevel()
        {
            if (levelEndHandler != null)
            {
                levelEndHandler.Deinitialize();
                levelEndHandler.OnEnded -= LevelEndHandler_OnEnded;
            }

            levelControllerService.Stage.OnStartChangeStage -= LevelStageController_OnStartChangeStage;
            levelControllerService.Stage.OnFinishChangeStage -= LevelStageController_OnFinishChangeStage;
            
            levelControllerService.OnLevelStateChanged -= LevelControllerService_OnLevelStateChanged;
            levelControllerService.Deinitialize();

            ClearObjects(levelObjects);

            levelEnvironment.Unload();

            Linker.ClearSavedLinks();

            ChangeState(LevelState.Unloaded);
        }


        //TODO temporary public
        public void ClearObjects(List<LevelObject> toClear)
        {
            toClear.ForEach(levelObject =>
            {
                levelObject.OnGameFinished -= LevelObject_OnGameFinished;
                levelObject.FinishGame();

                Content.Management.PoolHelper.PushObject(levelObject);
            });

            levelObjects.RemoveAll(e => toClear.Contains(e));
        }


        public void ChangeState(LevelState levelState)
        {
            if (CurrentState != levelState)
            {
                CurrentState = levelState;

                OnLevelStateChanged?.Invoke(CurrentState);
            }
        }


        private void EndLevel(LevelResult result)
        {
            levelControllerService.Announcer.Deinitialize();

            OnLevelEnd?.Invoke(result);
        }

        #endregion



        #region Events handlers

        private void LevelObject_OnGameFinished(LevelObject levelObject)
        {
            levelObject.OnGameFinished -= LevelObject_OnGameFinished;

            Content.Management.PoolHelper.PushObject(levelObject);

            levelObjects.Remove(levelObject);
        }


        private void LevelEndHandler_OnEnded(LevelResult levelResult)
        {
            EndLevel(levelResult);
        }

        private void LevelStageController_OnFinishChangeStage()
        {
            ChangeState(LevelState.Playing);
        }


        private void LevelStageController_OnStartChangeStage()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            ChangeState(LevelState.StageChanging);
        }


        private void LevelControllerService_OnLevelStateChanged(LevelState state) =>
            ChangeState(state);
        
        #endregion
    }
}
