using System;
using System.Collections.Generic;
using Drawmasters.Levels;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Helpers;
using Core;
using System.Collections;


namespace Drawmasters.LevelConstructor
{
    public class EnemyBossExtensionRocketDraw : MonoBehaviour
    {
        #region Nested Types
         
        [Serializable]
        public class Data
        {
            public ShooterColorType shooterColorType = default;

            public List<Vector3> rocketTrajectory = new List<Vector3>();

            public LineRenderer PathRenderer
            {
                get
                {
                    if (pathRenderer == null)
                    {
                        pathRenderer = Content.Management.CreateDefaultLineRenderer();
                    }

                    return pathRenderer;
                }
            }

            private LineRenderer pathRenderer;
            private UiRocketExtensionInfo uiRocketExtensionInfo;


            public void Initialize(UiRocketExtensionInfo infoPrefab, Transform infoRoot)
            {
                if (uiRocketExtensionInfo == null)
                {
                    uiRocketExtensionInfo = Instantiate(infoPrefab, infoRoot);
                }

                uiRocketExtensionInfo.Initialize(this);
                uiRocketExtensionInfo.SetupColorType(shooterColorType);
            }

            public void Deinitialize()
            {
                uiRocketExtensionInfo.Deinitialize();

                Destroy(uiRocketExtensionInfo.gameObject);
                uiRocketExtensionInfo = null;
            }


            public void RefreshIndex(int index) =>
                uiRocketExtensionInfo.RefreshIndex(index);

            public void RefreshLineGizmos(Color color)
            {
                PathRenderer.positionCount = rocketTrajectory.Count;
                PathRenderer.SetPositions(rocketTrajectory.ToArray());

                PathRenderer.colorGradient = GraphicsUtility.GetSolidGradient(color);
            }


            public void ClearLineGizmos()
            {
                PathRenderer.positionCount = 0;
                PathRenderer.SetPositions(Array.Empty<Vector3>());

                Destroy(pathRenderer.gameObject);
                pathRenderer = null;
            }
        }

        #endregion



        #region Fields

        public static event Action<bool> OnShouldSetSelectionLock;

        [SerializeField] private EnemyBossExtensionRocketDrawSettings settings = default;

        [SerializeField] private UiRocketExtensionInfo rocketInfoPrefab = default;
        [SerializeField] private Transform rocketInfoRoot = default;

        [SerializeField] private FloatInputUi toleranceInput = default;

        [SerializeField] private Button addRocketButton = default;

        [Header("Input")]
        [SerializeField] private KeyCode keyCodeToSimplify = default;

        [SerializeField] private KeyCode keyCodeToHold = default;
        [SerializeField] private KeyCode keyCodeToVerticalDirection = default;
        [SerializeField] private KeyCode keyCodeToHorizontalDirection = default;

        private readonly List<Data> currentStageRocketData = new List<Data>();

        private EditorEnemyBossLevelObject enemyBossLevelObject;

        private Camera editorCamera;

        private int currentStage;
        private Data currentRewdrawData;

        private InputMonitor inputMonitor;

        #endregion



        #region Unity lifecycle

        private void OnDisable()
        {
            if (Content.HasFoundInstance)
            {
                ClearLineGizmos();
            }
        }

        #endregion



        #region Methods

        public void Init(EditorEnemyBossLevelObject levelObject)
        {
            enemyBossLevelObject = levelObject;

            editorCamera = Camera.main;

            inputMonitor = inputMonitor ?? new InputMonitor(settings.pointsDistance);
            inputMonitor.MarkDrawFollowLine(editorCamera);

            currentStage = BossLevelInspector.CurrentStage;

            toleranceInput.Init("Tolerance", 1.0f);

            LoadDataFromObject();
            RefreshLineGizmos();
        }


        public void SubscribeOnEvents()
        {
            MonoBehaviourLifecycle.OnUpdate += OnUpdate;

            UiRocketExtensionInfo.OnShouldRemove += UiRocketExtensionInfo_OnShouldRemove;
            UiRocketExtensionInfo.OnShouldRedraw += UiRocketExtensionInfo_OnShouldRedraw;
            UiRocketExtensionInfo.OnShouldChangeColorType += UiRocketExtensionInfo_OnShouldChangeColorType;

            addRocketButton.onClick.AddListener(AddRocketButton_OnClick);

            EditorLevelStageChange.Subscribe(OnStageChanged);
        }

        public void UnsubscribeFromEvents()
        {
            MonoBehaviourLifecycle.OnUpdate -= OnUpdate;

            UiRocketExtensionInfo.OnShouldRemove -= UiRocketExtensionInfo_OnShouldRemove;
            UiRocketExtensionInfo.OnShouldRedraw -= UiRocketExtensionInfo_OnShouldRedraw;
            UiRocketExtensionInfo.OnShouldChangeColorType -= UiRocketExtensionInfo_OnShouldChangeColorType;

            addRocketButton.onClick.RemoveListener(AddRocketButton_OnClick);

            EditorLevelStageChange.Unsubscribe(OnStageChanged);
        }


        private void ClearLineGizmos()
        {
            foreach (var data in currentStageRocketData)
            {
                data.ClearLineGizmos();
            }
        }


        private void RefreshLineGizmos()
        { 
            foreach (var data in currentStageRocketData)
            {
                Color color = data == null ? Color.white : settings.GetColor(data.shooterColorType);
                data.RefreshLineGizmos(color);
            }
        }


        private void SavePointsData(int stage)
        {
            RocketLaunchData stageData = new RocketLaunchData();
            stageData.data = Array.Empty<RocketLaunchData.Data>();

            foreach (var currentStageData in currentStageRocketData)
            {
                RocketLaunchData.Data dataToSave = new RocketLaunchData.Data();
                dataToSave.colorType = currentStageData.shooterColorType;
                dataToSave.trajectory = currentStageData.rocketTrajectory.ToArray();

                stageData.data = stageData.data.Add(dataToSave);
            }

            enemyBossLevelObject.RocketLaunchData = enemyBossLevelObject.RocketLaunchData
                                                        .Put(stageData, e => Array.IndexOf(enemyBossLevelObject.RocketLaunchData, e) == stage);
            enemyBossLevelObject.RefreshSerializableData();
        }


        public void LoadDataFromObject()
        {
            RocketLaunchData[] loadedData = enemyBossLevelObject.RocketLaunchData;

            VisitAllCurrentStageData(e => e.Deinitialize());
            currentStageRocketData.Clear();

            if (loadedData.Length > currentStage)
            {
                RocketLaunchData loadedStageData = loadedData[currentStage];

                foreach (var data in loadedStageData.data)
                {
                    Data dataToAdd = new Data();

                    dataToAdd.shooterColorType = data.colorType;

                    foreach (var point in data.trajectory)
                    {
                        dataToAdd.rocketTrajectory.Add(point);
                    }

                    currentStageRocketData.Add(dataToAdd);
                    VisitAllCurrentStageData(e =>
                    {
                        e.Initialize(rocketInfoPrefab, rocketInfoRoot);
                        e.RefreshIndex(currentStageRocketData.IndexOf(e));
                    });
                }
            }
        }


        private void VisitAllCurrentStageData(Action<Data> callback)
        {
            foreach (var data in currentStageRocketData)
            {
                callback?.Invoke(data);
            }
        }

        #endregion



        #region Events handlers

        private void UiRocketExtensionInfo_OnShouldRedraw(Data targetData)
        {
            currentRewdrawData = targetData;
            inputMonitor.Clear();
            inputMonitor.StartMonitor();
            inputMonitor.MarkDrawFollowLine(editorCamera);

            StartCoroutine(StartMonitorMouseUp());
        }


        private void UiRocketExtensionInfo_OnShouldRemove(Data targetData)
        {
            ClearLineGizmos();

            if (targetData != null && currentStageRocketData.Count > 0)
            {
                targetData.Deinitialize();
                currentStageRocketData.Remove(targetData);

                VisitAllCurrentStageData(e => e.RefreshIndex(currentStageRocketData.IndexOf(e)));
            }

            RefreshLineGizmos();
            SavePointsData(currentStage);
        }


        private void UiRocketExtensionInfo_OnShouldChangeColorType(Data targetData, ShooterColorType targetColorType)
        {
            var dataToChange = currentStageRocketData.Find(e => e == targetData);

            if (dataToChange != null)
            {
                dataToChange.shooterColorType = targetColorType;
            }

            RefreshLineGizmos();
            SavePointsData(currentStage);
        }


        private void AddRocketButton_OnClick()
        {
            Data defaultData = new Data();

            currentStageRocketData.Add(defaultData);
            defaultData.Initialize(rocketInfoPrefab, rocketInfoRoot);
            defaultData.RefreshIndex(currentStageRocketData.Count - 1);

            currentRewdrawData = defaultData;

            inputMonitor.Clear();
            inputMonitor.StartMonitor();
            inputMonitor.MarkDrawFollowLine(editorCamera);

            StartCoroutine(StartMonitorMouseUp());

            RefreshLineGizmos();
            SavePointsData(currentStage);
        }

        private IEnumerator StartMonitorMouseUp()
        {
            OnShouldSetSelectionLock?.Invoke(true);

            yield return null;
            InputKeys.EventInputKeyUp.Subscribe(KeyCode.Mouse0, OnMouseUp);
        }


        private void OnMouseUp()
        {
            InputKeys.EventInputKeyUp.Unsubscribe(KeyCode.Mouse0, OnMouseUp);
            inputMonitor.StopMonitor();

            Data foundData = currentStageRocketData.Find(e => e == currentRewdrawData);

            if (foundData != null)
            {
                foundData.rocketTrajectory.Clear();
                foundData.rocketTrajectory.AddRange(inputMonitor.CurrentWorldPoints);
            }
            else
            {
                Debug.Log("Data that supposed to be redrawed not found");
            }

            RefreshLineGizmos();
            SavePointsData(currentStage);

            OnShouldSetSelectionLock?.Invoke(false);
        }


        private void OnStageChanged(int stage)
        {
            SavePointsData(currentStage);
            ClearLineGizmos();

            currentStage = stage;

            VisitAllCurrentStageData(e => e.Deinitialize());
            currentStageRocketData.Clear();

            LoadDataFromObject();
        }


        private void OnUpdate(float deltaTime)
        {
            if (Input.GetKey(keyCodeToHold))
            {
                if (Input.GetKey(keyCodeToVerticalDirection))
                {
                    inputMonitor.SetVerticalDirection(true);
                }

                if (Input.GetKey(keyCodeToHorizontalDirection))
                {
                    inputMonitor.SetHorizontalDirection(true);
                }

                if (Input.GetKeyDown(keyCodeToSimplify))
                {
                    inputMonitor.SimplifyTrajectory(toleranceInput.Value);
                }
            }

            if (Input.GetKeyUp(keyCodeToHold))
            {
                inputMonitor.SetVerticalDirection(false);
                inputMonitor.SetHorizontalDirection(false);
            }

            if (Input.GetKeyUp(keyCodeToVerticalDirection))
            {
                inputMonitor.SetVerticalDirection(false);
            }

            if (Input.GetKeyUp(keyCodeToHorizontalDirection))
            {
                inputMonitor.SetHorizontalDirection(false);
            }
        }

        #endregion
    }
}
