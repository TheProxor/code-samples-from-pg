using Drawmasters.Levels;
using Drawmasters.Pool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;


namespace Drawmasters.LevelConstructor
{
    public class PointsInputHandler : MonoBehaviour
    {
        #region Fields

        private const int PointInputsPreInstantiateCount = 5;

        [SerializeField] private PointInput pointInputPrefab = default;

        [SerializeField] private PointsHandlerController pointsHandlerController = default;

        private ComponentPool pointInputsPool;
        private IUpdatablePointStructure handlerUpdatablePointStructure;

        private readonly Dictionary<int, PointInput> pointInputsByIndex = new Dictionary<int, PointInput>();
        private int lastSelectedIndex;

        #endregion



        #region Properties

        public IUpdatablePointStructure HandlerUpdatablePointStructure
        {
            get => handlerUpdatablePointStructure;
            set
            {
                if (handlerUpdatablePointStructure != null)
                {
                    handlerUpdatablePointStructure.OnPointsUpdate -= HandlerUpdatablePointStructureOnPointsUpdate;
                }

                handlerUpdatablePointStructure = value;

                if (handlerUpdatablePointStructure != null)
                {
                    handlerUpdatablePointStructure.OnPointsUpdate += HandlerUpdatablePointStructureOnPointsUpdate;

                    UpdatePoints(handlerUpdatablePointStructure);
                }
            }
        }


        public int LastSelectedIndex
        {
            get => lastSelectedIndex;
            set
            {
                if (lastSelectedIndex != value)
                {

                    pointInputsByIndex.TryGetValue(lastSelectedIndex, out var selectedPointInput);
                    if (selectedPointInput != null)
                    {
                        selectedPointInput.SetSelected(false);
                    }

                    lastSelectedIndex = value;

                    pointInputsByIndex.TryGetValue(lastSelectedIndex, out selectedPointInput);
                    if (selectedPointInput != null)
                    {
                        selectedPointInput.SetSelected(true);
                    }
                }
            }
        }

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            InitializePool();
        }


        private void OnEnable()
        {
            PointInput.OnPointPositionChange += PointInput_OnPointPositionChange;
            PointInput.OnTargetModeButtonClick += PointInput_OnTargetModeButtonClick;

            pointsHandlerController.OnPointSelect += PointsHandlerController_OnPointSelect;
        }


        private void OnDisable()
        {
            PointInput.OnPointPositionChange -= PointInput_OnPointPositionChange;
            PointInput.OnTargetModeButtonClick -= PointInput_OnTargetModeButtonClick;

            pointsHandlerController.OnPointSelect -= PointsHandlerController_OnPointSelect;
        }

        #endregion



        #region Methods

        public void Initialize(EditorLevelObject editorLevelObject)
        {
            HandlerUpdatablePointStructure = editorLevelObject as IUpdatablePointStructure;

            if (handlerUpdatablePointStructure == null)
            {
                CustomDebug.LogError($"{editorLevelObject.name} has no implementation for interface {nameof(IUpdatablePointStructure)}");
            }

            LastSelectedIndex = -1;
        }


        private void InitializePool()
        {
            pointInputsPool = PoolManager.Instance.GetComponentPool(pointInputPrefab, true, PointInputsPreInstantiateCount);
        }


        private void UpdatePoints(IUpdatablePointStructure handlerUpdatablePointStructure)
        {
            List<PointData> handlerPointsData = handlerUpdatablePointStructure.PointData;

            foreach (PointData pointData in handlerPointsData)
            {
                pointInputsByIndex.TryGetValue(pointData.pointIndex, out var pointInput);

                if (pointInput != null)
                {
                    pointInput.UpdatePointData(pointData);
                }
                else
                {
                    CreatePointInput(pointData);
                }
            }
        }


        private void CreatePointInput(PointData pointData)
        {
            PointInput pointInput = pointInputsPool.Pop() as PointInput;

            if (pointInput == null)
            {
                CustomDebug.Log("Cannot load object from pool. Point pool prefab");
            }

            pointInput.transform.SetParent(transform);
            pointInput.transform.localScale = Vector3.one;
            pointInput.transform.localPosition = pointInput.transform.localPosition.SetZ(0.0f);

            if (pointInput != null)
            {
                pointInput.Initialize(pointData);
                pointInputsByIndex.Add(pointData.pointIndex, pointInput);
            }
        }

        #endregion



        #region Events handlers

        private void PointInput_OnTargetModeButtonClick(int index, ShapeTangentMode shapeTangentMode)
        {
            List<PointData> pointsData = handlerUpdatablePointStructure.PointData;
            PointData pointData = pointsData.Find(data => data.pointIndex == index);
            pointData.shapeTangentMode = shapeTangentMode;

            handlerUpdatablePointStructure.RefreshPoints(pointsData);
        }


        private void PointInput_OnPointPositionChange(int index, Vector3 newPosition)
        {
            List<PointData> pointsData = handlerUpdatablePointStructure.PointData;
            PointData pointData = pointsData.Find(data => data.pointIndex == index);
            pointData.pointPosition = newPosition;

            handlerUpdatablePointStructure.RefreshPoints(pointsData);
        }


        private void HandlerUpdatablePointStructureOnPointsUpdate(IUpdatablePointStructure updatablePointStructure)
        {
            if (HandlerUpdatablePointStructure == updatablePointStructure)
            {
                UpdatePoints(HandlerUpdatablePointStructure);
            }
        }


        private void PointsHandlerController_OnPointSelect(int index)
        {
            LastSelectedIndex = index;
        }

        #endregion
    }
}
