using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Helpers;
using TMPro;
using System.Linq;


namespace Drawmasters.LevelConstructor
{
    public class EnemyBossExtensionMove : MonoBehaviour
    {
        #region Nested Types

        [Serializable]
        public class Data
        {
            private LineRenderer pathRenderer;
            private DraggableObject pathPointPrefab;
            private Camera editorCamera;

            public List<Vector3> pathTrajectory = new List<Vector3>();
            public List<DraggableObject> pathPointObjects = new List<DraggableObject>();


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

            public void Initialize(Camera _editorCamera, DraggableObject _pathPointPrefab)
            {
                editorCamera = _editorCamera;
                pathPointPrefab = _pathPointPrefab;
            }


            public void RefreshLineRenderer()
            {
                PathRenderer.positionCount = pathTrajectory.Count;
                PathRenderer.SetPositions(pathTrajectory.ToArray());

                PathRenderer.colorGradient = GraphicsUtility.GetSolidGradient(Color.cyan);
            }


            public void RefreshGizmos()
            {
                ClearGizmos();

                for (int i = 0; i < pathTrajectory.Count; i++)
                {
                    DraggableObject pointObject = Instantiate(pathPointPrefab);
                    pointObject.GetComponentInChildren<TextMeshPro>().text = i.ToString();

                    pointObject.transform.position = pathTrajectory[i];

                    pointObject.SetupCamera(editorCamera);

                    pathPointObjects.Add(pointObject);

                    pointObject.OnDragged += PointObject_OnDragged;
                }

                RefreshLineRenderer();
            }


            private void PointObject_OnDragged(DraggableObject obj)
            {
                RewritePoints();
                RefreshLineRenderer();
            }

            public void RewritePoints() =>
                pathTrajectory = pathPointObjects.Select(e => e.transform.position).ToList();
            

            public void ClearGizmos()
            {
                foreach (var pointObject in pathPointObjects)
                {
                    if (pointObject != null)
                    {
                        if (pointObject.gameObject != null)
                        {
                            pointObject.OnDragged -= PointObject_OnDragged;
                            Destroy(pointObject.gameObject);
                        }
                    }
                }

                pathPointObjects.Clear();
                RefreshLineRenderer();

                Destroy(pathRenderer.gameObject);
                pathRenderer = null;
            }
        }

        #endregion



        #region Fields

        [SerializeField] private DraggableObject pathPointPrefab = default;

        [SerializeField] private Button addPointButton = default;
        [SerializeField] private Button clearPointsButton = default;

        private readonly Data data = new Data();

        private EditorEnemyBossLevelObject enemyBossLevelObject;

        private Camera editorCamera;
        private int currentStage;

        #endregion



        #region Unity lifecycle

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            data.ClearGizmos();
        }

        #endregion



        #region Methods

        public void Init(EditorEnemyBossLevelObject levelObject)
        {
            enemyBossLevelObject = levelObject;

            editorCamera = Camera.main;
            currentStage = BossLevelInspector.CurrentStage;

            data.Initialize(editorCamera, pathPointPrefab);

            LoadDataFromObject();
            RefreshLineGizmos();
        }


        public void SubscribeOnEvents()
        {
            EditorLevel.OnPreSaveStage += EditorLevel_OnPreSaveStage;
            EditorLevelStageChange.Subscribe(OnStageChanged);

            addPointButton.onClick.AddListener(AddPointButton_OnClick);
            clearPointsButton.onClick.AddListener(ClearPointsButton_OnClick);
        }


        public void UnsubscribeFromEvents()
        {
            EditorLevel.OnPreSaveStage -= EditorLevel_OnPreSaveStage;
            EditorLevelStageChange.Unsubscribe(OnStageChanged);

            addPointButton.onClick.RemoveListener(AddPointButton_OnClick);
            clearPointsButton.onClick.RemoveListener(ClearPointsButton_OnClick);
        }


        private void RefreshLineGizmos()
        {
            data.ClearGizmos();
            data.RefreshGizmos();
        }


        public void LoadDataFromObject()
        {
            LevelObjectMoveSettings[] loadedData = enemyBossLevelObject.StagesMovement;

            data.ClearGizmos();

            if (loadedData.Length > currentStage)
            {
                LevelObjectMoveSettings loadedStageData = loadedData[currentStage];

                data.pathTrajectory = new List<Vector3>(loadedStageData.path);
            }

            data.RefreshGizmos();
            data.RewritePoints();
        }

        #endregion



        #region Events handlers

        private void AddPointButton_OnClick()
        {
            data.pathTrajectory.Add(Vector3.zero);

            RefreshLineGizmos();
        }


        private void ClearPointsButton_OnClick()
        {
            if (data.pathTrajectory.IsNullOrEmpty())
            {
                return;
            }

            data.pathTrajectory.Clear();

            RefreshLineGizmos();
        }


        private void OnStageChanged(int stage)
        {
            currentStage = stage;

            data.ClearGizmos();
        }


        private void EditorLevel_OnPreSaveStage(int stage)
        {
            LevelObjectMoveSettings stageData = new LevelObjectMoveSettings();

            if (!data.pathPointObjects.IsNullOrEmpty())
            {
                data.pathPointObjects.Last().transform.position = enemyBossLevelObject.transform.position;
            }

            data.RewritePoints();
            data.RefreshGizmos();

            stageData.path = new List<Vector3>(data.pathTrajectory);

            enemyBossLevelObject.StagesMovement = enemyBossLevelObject.StagesMovement
                                                        .Put(stageData, e => Array.IndexOf(enemyBossLevelObject.StagesMovement, e) == stage);
            enemyBossLevelObject.RefreshSerializableData();
        }

        #endregion
    }
}
