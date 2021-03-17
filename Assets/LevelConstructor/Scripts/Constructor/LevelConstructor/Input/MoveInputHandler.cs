using System.Collections.Generic;
using Modules.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class MoveInputHandler : MonoBehaviour
    {
        #region Fields

        [SerializeField] private DraggableObject pathPointPrefab = default;

        [SerializeField] private Vector3UI pathPointInputPrefab = default;
        [SerializeField] private Transform pathPointsRoot = default;

        [SerializeField] private FloatInputUi delayBetweenPointsField = default;
        [SerializeField] private FloatInputUi durationMoveBetweenPointsField = default;

        [SerializeField] private Button addPathPointButton = default;

        [SerializeField] private FloatInputUi removePathPointField = default;
        [SerializeField] private Button removePathPointButton = default;

        [SerializeField] private BoolInputUi tweenTypeInput = default;

        [SerializeField] private LineRenderer pathRenderer = default;

        private List<Vector3UI> createdPointInputs = new List<Vector3UI>();
        private List<DraggableObject> pathPointObjects = new List<DraggableObject>();

        private EditorLevelObject selectedObject;
        private LevelObjectMoveSettings moveSettings;

        private Camera editorCamera;

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            removePathPointField.Init("Remove index", 0.0f, 0.0f);

            addPathPointButton.onClick.AddListener(AddPoint);
            removePathPointButton.onClick.AddListener(RemovePoint);
        }


        private void OnDisable()
        {
            delayBetweenPointsField.OnValueChange -= DelayBetweenPointsField_OnValueChange;
            durationMoveBetweenPointsField.OnValueChange -= DurationMoveBetweenPointsField_OnValueChange;
            tweenTypeInput.OnValueChange -= TweenTypeInput_OnValueChange;

            addPathPointButton.onClick.RemoveListener(AddPoint);
            removePathPointButton.onClick.RemoveListener(RemovePoint);

            ClearPointPathGizmos();
            ClearLinePathGizmos();
        }


        private void Update()
        {
            SetupFirstPoint();
            RefreshLinePathGizmos();
        }

        #endregion



        #region Methods

        public void Initialize(EditorLevelObject _selectedObject)
        {
            selectedObject = _selectedObject;
            moveSettings = selectedObject.MoveSettings;

            editorCamera = Camera.main;

            delayBetweenPointsField.Init("Points delay", moveSettings.delayBetweenPoints);
            delayBetweenPointsField.OnValueChange += DelayBetweenPointsField_OnValueChange;

            durationMoveBetweenPointsField.Init("Total duration", moveSettings.totalMoveDuration);
            durationMoveBetweenPointsField.OnValueChange += DurationMoveBetweenPointsField_OnValueChange;

            tweenTypeInput.Init("Is cycled", moveSettings.isCycled);
            tweenTypeInput.OnValueChange += TweenTypeInput_OnValueChange;

            foreach (var input in createdPointInputs)
            {
                Destroy(input.gameObject);
            }

            createdPointInputs.Clear();

            for (int i = 0; i < moveSettings.path.Count; i++)
            {
                Vector3UI pathPoint = Instantiate(pathPointInputPrefab, pathPointsRoot);

                pathPoint.Init($"Path point {i}", 2);
                pathPoint.SetCurrentValue(moveSettings.path[i]);

                pathPoint.OnValueChange += PathPoint_OnValueChange;

                createdPointInputs.Add(pathPoint);
            }

            // Cuz of editor object strange lifecycle
            Scheduler.Instance.CallMethodWithDelay(this, RefreshPointPathGizmos, CommonUtility.OneFrameDelay);
        }


        private void SavePointsData()
        {
            moveSettings.path = new List<Vector3>(createdPointInputs.Count);

            foreach (var i in createdPointInputs)
            {
                moveSettings.path.Add(i.CurrentValue);
            }
        }


        private void RefreshLinePathGizmos()
        {
            if (moveSettings != null &&
                moveSettings.CanMove)
            {
                pathRenderer.positionCount = moveSettings.path.Count;
                pathRenderer.SetPositions(moveSettings.path.ToArray());
            }
        }


        private void ClearLinePathGizmos()
        {
            if (pathRenderer != null)
            {
                pathRenderer.positionCount = 0;
            }
        }


        private void RefreshPointPathGizmos()
        {
            ClearPointPathGizmos();

            for (int i = 0; i < moveSettings.path.Count; i++)
            {
                Vector3 pathPoint = moveSettings.path[i];

                DraggableObject pointObject = Instantiate(pathPointPrefab);
                pointObject.GetComponentInChildren<TextMeshPro>().text = i.ToString();

                pointObject.transform.position = pathPoint;

                pointObject.SetupCamera(editorCamera);

                pointObject.OnDragged += DraggableObject_OnDragged;
                pointObject.OnEndDragging += SavePointsData;

                pathPointObjects.Add(pointObject);
            }
        }


        private void ClearPointPathGizmos()
        {
            foreach (var pointObject in pathPointObjects)
            {
                if (pointObject != null)
                {
                    pointObject.OnDragged -= DraggableObject_OnDragged;
                    pointObject.OnEndDragging -= SavePointsData;

                    if (pointObject.gameObject != null)
                    {
                        Destroy(pointObject.gameObject);
                    }
                }
            }

            pathPointObjects.Clear();
        }


        private void SetupFirstPoint()
        {
            if (createdPointInputs.Count > 0 &&
                pathPointObjects.Count > 0)
            {
                createdPointInputs[0].SetCurrentValue(selectedObject.transform.position);

                if (moveSettings.path.Count == 0)
                {
                    moveSettings.path.Add(Vector3.zero);
                }

                moveSettings.path[0] = createdPointInputs[0].CurrentValue;
                pathPointObjects[0].transform.position = createdPointInputs[0].CurrentValue;

            }
        }

        #endregion



        #region Events handlers

        private void AddPoint()
        {
            Vector3UI pathPoint = Instantiate(pathPointInputPrefab, pathPointsRoot);

            pathPoint.Init("Point", 2);
            pathPoint.OnValueChange += PathPoint_OnValueChange;

            createdPointInputs.Add(pathPoint);

            SavePointsData();

            RefreshLinePathGizmos();
            RefreshPointPathGizmos();
        }


        private void RemovePoint()
        {
            int choosedIndexToRemove = (int)removePathPointField.Value;

            if (choosedIndexToRemove >= 0 &&
                choosedIndexToRemove < createdPointInputs.Count)
            {
                Vector3UI pathPoint = createdPointInputs[choosedIndexToRemove];
                pathPoint.OnValueChange -= PathPoint_OnValueChange;

                createdPointInputs.Remove(pathPoint);

                Destroy(pathPoint.gameObject);
            }

            SavePointsData();

            RefreshLinePathGizmos();
            RefreshPointPathGizmos();
        }


        private void DraggableObject_OnDragged(DraggableObject obj)
        {
            int refIndex = pathPointObjects.IndexOf(obj);

            if (refIndex != -1)
            {
                createdPointInputs[refIndex].SetCurrentValue(obj.transform.position);
                moveSettings.path[refIndex] = obj.transform.position;

                RefreshLinePathGizmos();
            }
        }


        private void PathPoint_OnValueChange(Vector3 value)
        {
            SavePointsData();

            RefreshPointPathGizmos();
            RefreshLinePathGizmos();
        }


        private void DurationMoveBetweenPointsField_OnValueChange(float value)
        {
            value = Mathf.Max(0.0f, value);
            moveSettings.totalMoveDuration = value;
        }


        private void DelayBetweenPointsField_OnValueChange(float value)
        {
            value = Mathf.Max(0.0f, value);
            moveSettings.delayBetweenPoints = value;
        }


        private void TweenTypeInput_OnValueChange(bool value)
        {
            moveSettings.isCycled = value;
        }

        #endregion
    }
}
