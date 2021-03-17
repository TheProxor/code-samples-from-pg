using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Drawmasters.Levels;
using Drawmasters.LevelsRepository;
using Drawmasters.LevelsRepository.Editor;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorLevel : MonoBehaviour
    {
        #region Fields

        public static event Action<int> OnPreSaveStage;

        static readonly Vector3 DefaultEditorCameraPosition = Vector3.zero;

        [SerializeField] private CameraMovement gamePreviewCamera = null;
        [SerializeField] private EditorCameraMovement editorCamera = null;
        [SerializeField] private Transform objectsRoot = null;
        [Header("Prefab for drawing gizmos on bonus level")]
        [SerializeField] private BonusLevelTrajectoryDrawer drawerPrefab = default;

        private ObservableCollection<EditorLevelObject> levelObjects = new ObservableCollection<EditorLevelObject>();

        private LevelHeader header;

        private int editorObjectsLayer;
        private int currentStageIndex = 1;

        private int currentBonusLevelStage;

        private readonly List<BonusLevelTrajectoryDrawer> drawers = new List<BonusLevelTrajectoryDrawer>();

        #endregion



        #region Properties

        public CameraMovement GamePreviewCamera => gamePreviewCamera;


        public bool IsPhysicsEnabled { get; private set; }


        public bool IsCollisionEnabled { get; private set; }

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            editorObjectsLayer = LayerMask.NameToLayer("EditorLevelObjects");
            SetCollisionEnabled(IsCollisionEnabled);
            
            levelObjects.CollectionChanged += LevelObjects_OnCollectionChanged;
        }


        private void OnDestroy()
        {
            SetCollisionEnabled(false);
            
            foreach(var i in drawers)
            {
                if (!i.IsNull())
                {
                    Destroy(i.gameObject);
                }
            }

            drawers.Clear();
        }

        #endregion



        #region Methods

        public void Load(LevelHeader header)
        {
            this.header = header;
            Clear();

            var levelData = ConstructorLevelsManager.GetLevelData(header);
            levelData.levelObjectsData.ForEach((data) => CreateObject(data.index, data));
            EditorLinker.Init(levelData.linkerData, levelObjects.ToList());

            Vector3 cameraPosition = DefaultEditorCameraPosition;

            editorCamera.Position = cameraPosition;
            editorCamera.Rotation = Quaternion.identity;
            editorCamera.Init();
        }


        public void RemoveObject(EditorLevelObject objectToRemove)
        {
            levelObjects.Remove(objectToRemove);
            EditorLinker.RemoveObject(objectToRemove);
            Destroy(objectToRemove.gameObject);
        }


        public void Clear()
        {
            levelObjects.ToList().ForEach((levelObject) => Destroy(levelObject.gameObject));
            levelObjects.Clear();
            drawers.ForEach(i =>
            {
                i.StopDraw();
                
                Destroy(i.gameObject);
            });
            drawers.Clear();
            
            EditorLinker.Clear();
        }


        public EditorLevelObject SpawnNewObject(int index, LevelObjectData data = null)
        {
            data.createdStageIndex = currentStageIndex;
            
            data.bonusData = new BonusLevelObjectData
            {
                acceleration = 50f,
                angularVelocity = 15,
                linearVelocity = Vector2.up * 200f,
                stageIndex = BonusLevelInspector.CurrentStage,
            };
            
            EditorLevelObject levelObject = CreateObject(index, data);

            return levelObject;
        }


        public void Save()
        {
            List<LevelObjectData> levelObjectsData = new List<LevelObjectData>();

            List<LevelObjectData> currentObjectsData = ConstructorLevelsManager.GetLevelData(header).levelObjectsData;

            foreach (var levelObject in levelObjects)
            {
                LevelObjectData currentData = currentObjectsData.Find(e => levelObject.IsDataEqual(e));

                if (currentData == null)
                {
                    Debug.Log($"No stages found in {this} for object {levelObject}");
                }

                var data = levelObject.GetData();
                data.stageData = currentData == null ? new List<StageLevelObjectData>() : currentData.stageData;

                levelObjectsData.Add(data);
                levelObject.SetData(data);
            }

            ConstructorLevelsManager.SetLevelData(header, new Level.Data
            {
                levelObjectsData = levelObjectsData,
                linkerData = EditorLinker.GetSerializableLinks(levelObjects.ToList()),
                pathDistance = ConstructorLevelsManager.GetLevelData(header).pathDistance
            });
        }


        public void SaveStage(int stageIndex)
        {
            OnPreSaveStage?.Invoke(stageIndex);

            List<LevelObjectData> levelObjectsData = ConstructorLevelsManager.GetLevelData(header).levelObjectsData;

            foreach (var editorLevelObject in levelObjects)
            {
                int foundIndex = levelObjectsData.FindIndex(e => editorLevelObject.IsDataEqual(e));

                if (foundIndex == -1)
                {
                    Debug.Log($"No data found in {this} for object {editorLevelObject}");
                    continue;
                }

                LevelObjectData objectData = editorLevelObject.GetData();
                objectData.stageData = levelObjectsData[foundIndex].stageData;
                objectData.stageData = GetStageData(editorLevelObject, objectData, objectData.stageData);

                levelObjectsData[foundIndex] = objectData;

                editorLevelObject.SetData(objectData);
            }

            ConstructorLevelsManager.SetLevelData(header, new Level.Data
            {
                levelObjectsData = levelObjectsData,
                linkerData = EditorLinker.GetSerializableLinks(levelObjects.ToList()),
                pathDistance = ConstructorLevelsManager.GetLevelData(header).pathDistance
            });


            List<StageLevelObjectData> GetStageData(EditorLevelObject editorLevelObject, LevelObjectData objectData, List<StageLevelObjectData> existsStageLevelObjectData)
            {
                List<StageLevelObjectData> result = objectData.stageData;
                result = result ?? new List<StageLevelObjectData>();

                for (int i = result.Count; i < header.stagesCount; i++)
                {
                    result.Add(new StageLevelObjectData(objectData));
                }

                StageLevelObjectData stageData = new StageLevelObjectData(objectData);

                if (existsStageLevelObjectData != null && stageIndex < existsStageLevelObjectData.Count)
                {
                    stageData.isFreeFall = existsStageLevelObjectData[stageIndex].isFreeFall;
                }

                if ((stageIndex + 1) < result.Count)
                {
                    result[stageIndex + 1].isFreeFall = editorLevelObject.IsNextStageFreeFall;
                }

                result[stageIndex] = stageData;
                result[stageIndex].comeVelocity = editorLevelObject.ComeVelocity;

                bool shouldSaveTransform = editorLevelObject is EditorLevelTargetObject;
                if (!shouldSaveTransform)
                {
                    foreach (var d in result)
                    {
                        d.position = stageData.position;
                        d.rotation = stageData.rotation;
                    }
                }

                return result;
            }
        }

        public void ChangeBonusStage(int stage)
        {
            currentBonusLevelStage = stage;
            
            //HACK
            LevelObjects_OnCollectionChanged(null, null);
            
            List<LevelObjectData> levelObjectsData = ConstructorLevelsManager.GetLevelData(header).levelObjectsData;

            for (int i = 0; i < levelObjects.Count; i++)
            {
                EditorLevelObject levelObject = levelObjects[i];

                LevelObjectData equalData = levelObjectsData.Find(e => levelObject.IsDataEqual(e));

                if (equalData == null || !(levelObject is EditorPhysicalLevelObject))
                { 
                    continue;
                }

                bool isActive = levelObject.BonusLevelStageIndex == currentBonusLevelStage;
                
                levelObject.gameObject.SetActive(isActive);
            }
        }


        public void ChangeStage(int stage)
        {
            currentStageIndex = stage;

            List<LevelObjectData> levelObjectsData = ConstructorLevelsManager.GetLevelData(header).levelObjectsData;

            for (int i = 0; i < levelObjects.Count; i++)
            {
                EditorLevelObject levelObject = levelObjects[i];

                LevelObjectData equalData = levelObjectsData.Find(e => levelObject.IsDataEqual(e));

                if (equalData == null)
                {
                    Debug.Log($"No data found in {this} for object {levelObject}");
                    continue;
                }

                if (equalData.stageData != null &&
                    currentStageIndex < equalData.stageData.Count)
                {
                    if (currentStageIndex + 1 < equalData.stageData.Count)
                    {
                        levelObject.IsNextStageFreeFall = equalData.stageData[currentStageIndex + 1].isFreeFall;
                    }

                    levelObject.SetStageData(equalData.stageData[currentStageIndex]);

                    int objectFallIndex = equalData.stageData.FindIndex(e => e.isFreeFall);

                    bool shouldShowObject = currentStageIndex >= equalData.createdStageIndex &&
                                            (currentStageIndex < objectFallIndex || objectFallIndex == -1);

                    levelObject.gameObject.SetActive(shouldShowObject);
                }
            }

            EditorLevelStageChange.Register(currentStageIndex);
        }


        public void SetPhysicsEnabled(bool isEnabled)
        {
            IsPhysicsEnabled = isEnabled;
            levelObjects.ToList().ForEach((levelObject) => levelObject.SetPhysicsEnabled(IsPhysicsEnabled));
        }


        public void SetCollisionEnabled(bool isEnabled)
        {
            IsCollisionEnabled = isEnabled;
            Physics2D.IgnoreLayerCollision(editorObjectsLayer, editorObjectsLayer, !IsCollisionEnabled);
        }


        private EditorLevelObject CreateObject(int index, in LevelObjectData data)
        {
            EditorLevelObject levelObject = EditorObjectsContainer.Create(index);

            if (levelObject == null)
            {
                return null;
            }

            levelObject.transform.SetParent(objectsRoot);
            levelObject.TryAddRigidbody();

            if (string.IsNullOrEmpty(data.additionalInfo))
            {
                LevelObjectData loadedData = levelObject.GetData();
                data.additionalInfo = loadedData.additionalInfo;
            }

            levelObject.SetData(data);

            levelObject.SetPhysicsEnabled(false);
            levelObjects.Add(levelObject);

            return levelObject;
        }


        private bool IsMatchForGizmos(EditorLevelObject editorLevelObject)
        {
            bool isPhysical = editorLevelObject is EditorPhysicalLevelObject;
            bool isForCurrentStage = editorLevelObject.BonusLevelStageIndex == currentBonusLevelStage;
            
            return isPhysical && isForCurrentStage;
        }

        #endregion
        
        
        
        #region Events handlers
        
        private void LevelObjects_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (header == null || 
                header.levelType != LevelType.Bonus)
            {
                return;
            }
            
            
            foreach (var editorLevelObject in levelObjects)
            {
                bool isMatch = IsMatchForGizmos(editorLevelObject);
                if (!isMatch)
                {
                    for (int i = drawers.Count - 1; i >= 0; i--)
                    {
                        BonusLevelTrajectoryDrawer drawer = drawers[i];

                        if (drawer.LevelObject == editorLevelObject)
                        {
                            drawer.StopDraw();
                            Destroy(drawer.gameObject);
                            drawers.RemoveAt(i);
                        }
                    }
                    
                    continue;
                }

                bool isExist = drawers.Any(i => i.LevelObject == editorLevelObject);
                if (!isExist)
                {
                    BonusLevelTrajectoryDrawer drawer = Instantiate(drawerPrefab);

                    drawer.StartDraw(editorLevelObject);
                    
                    drawers.Add(drawer);
                }
            }
            
            
        }
        
        #endregion
    }
}
