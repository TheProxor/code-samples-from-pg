using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Drawmasters.Levels;
using Drawmasters.Pool;
using Serialization;
using UnityEngine;
using UnityEngine.U2D;


namespace Drawmasters.LevelConstructor
{
    public class PointsHandlerController : BaseHandlersController
    {
        #region Nested Types

        private class ShapePointInfo
        {
            public int index = default;
            public PointHandler pointHandler = default;
            public ShapeTangentMode shapeTangentMode = default;


            public ShapePointInfo(int index, PointHandler pointHandler, ShapeTangentMode shapeTangentMode)
            {
                this.index = index;
                this.pointHandler = pointHandler;
                this.shapeTangentMode = shapeTangentMode;
            }
        }


        private class PointableObjectInfo
        {
            public IUpdatablePointStructure UpdatablePointStructureObject;
            public Transform objectTransform;


            public PointableObjectInfo(IUpdatablePointStructure updatablePointStructureObject, Transform objectTransform)
            {
                this.UpdatablePointStructureObject = updatablePointStructureObject;
                this.objectTransform = objectTransform;
            }
        }

        #endregion



        #region Fields

        private const int HandlersPreInstantiateCount = 5;

        public event Action<int> OnPointSelect;

        [SerializeField] private HandlerInfo pointHandlersInfo = default;
        [SerializeField] private KeyCode pointDeleteKey = default;
        [SerializeField] private KeyCode pointCreateKey = default;

        private readonly Dictionary<PointableObjectInfo, List<ShapePointInfo>> shapePointInfosByIPointable =
            new Dictionary<PointableObjectInfo, List<ShapePointInfo>>();

        private ComponentPool pointsPool = default;

        private PointableObjectInfo activePointable = null;

        #endregion



        #region Unity lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();

            PointHandler.OnPositionChange += PointHandler_OnPositionChange;

            InputKeys.EventInputKeyDown.Subscribe(pointDeleteKey, PointDeleteKey_OnDown);
            InputKeys.EventInputKeyDown.Subscribe(pointCreateKey, PointCreateKey_OnDown);
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            PointHandler.OnPositionChange -= PointHandler_OnPositionChange;

            InputKeys.EventInputKeyDown.Unsubscribe(pointDeleteKey, PointDeleteKey_OnDown);
            InputKeys.EventInputKeyDown.Unsubscribe(pointCreateKey, PointCreateKey_OnDown);
        }

        #endregion



        #region Methods

        private void InitializePool()
        {
            pointsPool = PoolManager.Instance.GetComponentPool(handlerPrefab, true, HandlersPreInstantiateCount);
        }


        private PointHandler CreateEditorPointObject(Vector3 point, Transform parentTransform)
        {
            PointHandler result = pointsPool.Pop() as PointHandler;

            if (result == null)
            {
                CustomDebug.Log("Cannot load object from pool. Point handler prefab.");
            }

            result.transform.SetParent(parentTransform);
            result.transform.localPosition = point;

            if (result != null)
            {
                float snapSize = (objectsGridDrawer.IsGridAvailable) ? objectsGridDrawer.SnapSize : handlersSnapSize;

                result.Initialize(pointHandlersInfo.direction, pointHandlersInfo.unSelectedColor,
                    pointHandlersInfo.selectedColor, snapSize, selectedObjects);
            }

            return result;
        }


        private void GeneratePoints(PointableObjectInfo pointableObjectInfo)
        {
            Vector3 pointPosition = Vector3.zero;
            PointHandler pointHandler = default;

            List<PointData> pointsData = pointableObjectInfo.UpdatablePointStructureObject.PointData;
            List<ShapePointInfo> shapePointInfos = new List<ShapePointInfo>();
            pointableObjectInfo.UpdatablePointStructureObject.OnPointsUpdate += UpdatablePointStructureOnPointsUpdate;

            for (int i = 0, n = pointsData.Count; i < n; i++)
            {
                pointPosition = pointsData[i].pointPosition;
                pointHandler = CreateEditorPointObject(pointPosition, pointableObjectInfo.objectTransform);

                shapePointInfos.Add(new ShapePointInfo(pointsData[i].pointIndex, pointHandler, pointsData[i].shapeTangentMode));
            }

            shapePointInfosByIPointable.Add(pointableObjectInfo, shapePointInfos);
        }


        private void RemovePoints(PointableObjectInfo pointable)
        {
            List<ShapePointInfo> shapePointInfos = null;
            shapePointInfosByIPointable.TryGetValue(pointable, out shapePointInfos);

            if (shapePointInfos != null)
            {
                foreach (ShapePointInfo shapePointInfo in shapePointInfos)
                {
                    Destroy(shapePointInfo.pointHandler.gameObject);
                }

                pointable.UpdatablePointStructureObject.OnPointsUpdate -= UpdatablePointStructureOnPointsUpdate;
                shapePointInfosByIPointable.Remove(pointable);
            }
        }


        private void UpdatePointsData(PointableObjectInfo pointable)
        {
            List<PointData> pointsData = new List<PointData>();
            List<ShapePointInfo> shapePointInfos = new List<ShapePointInfo>();
            shapePointInfosByIPointable.TryGetValue(pointable, out shapePointInfos);

            if (shapePointInfos != null)
            {
                foreach (ShapePointInfo shapePointInfo in shapePointInfos)
                {
                    pointsData.Add(new PointData(shapePointInfo.index,
                        shapePointInfo.pointHandler.transform.localPosition, shapePointInfo.shapeTangentMode));
                }

                pointable.UpdatablePointStructureObject.OnPointsUpdate -= UpdatablePointStructureOnPointsUpdate;

                pointable.UpdatablePointStructureObject.RefreshPoints(pointsData);

                pointable.UpdatablePointStructureObject.OnPointsUpdate += UpdatablePointStructureOnPointsUpdate;
            }
        }


        private int CalculatePointIndex(Vector2 pointPosition, List<ShapePointInfo> shapePointInfos)
        {
            var nearestPointsInfo = (leftIndex: 0, rightIndex: 0, distance: float.MaxValue);
            for (int i = 0, n = shapePointInfos.Count; i < n; i++)
            {
                int leftIndex = i;
                int rightIndex = (i < n - 1) ? (i + 1) : (0);
                ShapePointInfo leftShapePointInfo = shapePointInfos.Find((info => info.index == leftIndex));
                ShapePointInfo rightShapePointInfo = shapePointInfos.Find((info => info.index == rightIndex));
                float distance = CommonUtility.CalculateDistanceFromPointToSegment(pointPosition,
                    leftShapePointInfo.pointHandler.transform.position.ToVector2(),
                    rightShapePointInfo.pointHandler.transform.position.ToVector2());

                if (nearestPointsInfo.distance > distance)
                {
                    nearestPointsInfo = (leftIndex, rightIndex, distance);
                }
            }

            return nearestPointsInfo.rightIndex;
        }

        #endregion



        #region Events handlers

        private void PointHandler_OnPositionChange(PointHandler pointHandler)
        {
            UpdatePointsData(activePointable);
        }


        private void PointDeleteKey_OnDown()
        {
            if (activeHandler != null)
            {
                List<ShapePointInfo> shapePointInfos = new List<ShapePointInfo>();
                shapePointInfosByIPointable.TryGetValue(activePointable, out shapePointInfos);

                if (shapePointInfos != null)
                {
                    ShapePointInfo shapePointInfo = shapePointInfos.
                        Find((info => info.pointHandler == activeHandler));

                    activePointable.UpdatablePointStructureObject.RemovePoint(shapePointInfo.index);
                    RemovePoints(activePointable);
                    GeneratePoints(activePointable);

                    activeHandler = null;
                    activePointable = null;
                }
            }
        }


        private void PointCreateKey_OnDown()
        {
            if (IsShowing)
            {
                List<ShapePointInfo> shapePointInfos = null;
                shapePointInfosByIPointable.TryGetValue(activePointable, out shapePointInfos);

                if (shapePointInfos != null)
                {
                    Vector3 inputScreenPosition = activeCamera.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 pointPosition = Vector3.zero;

                    if (objectsGridDrawer.IsGridAvailable)
                    {
                        pointPosition = objectsGridDrawer.GetNearestPoint(inputScreenPosition);
                    }
                    else
                    {
                        pointPosition = inputScreenPosition.SetZ(handlerPrefab.transform.position.z);
                    }

                    int pointIndex = CalculatePointIndex(pointPosition, shapePointInfos);

                    activePointable.UpdatablePointStructureObject.InsertPoint(pointIndex, pointPosition);

                    RemovePoints(activePointable);
                    GeneratePoints(activePointable);
                }
            }
        }


        private void UpdatablePointStructureOnPointsUpdate(IUpdatablePointStructure updatablePointStructure)
        {
            PointableObjectInfo pointableObjectInfo =
                shapePointInfosByIPointable.Keys.LastOrDefault(info => info.UpdatablePointStructureObject.Equals(updatablePointStructure));

            if (pointableObjectInfo != null)
            {
                RemovePoints(pointableObjectInfo);
                GeneratePoints(pointableObjectInfo);
            }
        }

        #endregion



        #region BaseHandlersController

        public override void Initialize(Camera editorCamera, ObjectsGridDrawer objectsGridDrawer)
        {
            base.Initialize(editorCamera, objectsGridDrawer);

            InitializePool();
        }


        public override bool IsHandlerBelongsToController(BaseHandler baseHandler)
        {
            foreach (List<ShapePointInfo> shapePointInfos in shapePointInfosByIPointable.Values)
            {
                foreach (ShapePointInfo shapePointInfo in shapePointInfos)
                {
                    if (shapePointInfo.pointHandler.Equals(baseHandler))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public override void ChangeActiveHandlers(BaseHandler activeHandler)
        {
            base.ChangeActiveHandlers(activeHandler);

            foreach (KeyValuePair<PointableObjectInfo, List<ShapePointInfo>> shapePointInfos
                in shapePointInfosByIPointable)
            {
                foreach (ShapePointInfo shapePointInfo in shapePointInfos.Value)
                {
                    if (shapePointInfo.pointHandler.Equals(activeHandler))
                    {
                        shapePointInfo.pointHandler.ChangeSelection(true);
                        activePointable = shapePointInfos.Key;

                        OnPointSelect?.Invoke(shapePointInfo.index);
                    }
                    else
                    {
                        shapePointInfo.pointHandler.ChangeSelection(false);
                    }
                }
            }
        }


        public override void SetSelectedObject(List<EditorLevelObject> editorLevelObjects)
        {
            base.SetSelectedObject(editorLevelObjects);

            IsShowing = editorLevelObjects != null && editorLevelObjects.Count > 0;

            List<EditorLevelObject> editorPointableLevelObjects =
                editorLevelObjects.FindAll((o => o is IUpdatablePointStructure)).ToList();
            List<IUpdatablePointStructure> iPointableObjects =
                editorPointableLevelObjects.Cast<IUpdatablePointStructure>().ToList();

            for (int i = shapePointInfosByIPointable.Count, n = 0; i > n; i--)
            {
                if (!iPointableObjects.Contains(shapePointInfosByIPointable.Keys.ToArray()[i - 1].UpdatablePointStructureObject))
                {
                    RemovePoints(shapePointInfosByIPointable.Keys.ToArray()[i - 1]);
                }
            }

            foreach (EditorLevelObject editorLevelObject in editorPointableLevelObjects)
            {
                IUpdatablePointStructure iUpdatablePointStructure = editorLevelObject as IUpdatablePointStructure;
                if (shapePointInfosByIPointable.Keys.LastOrDefault(info => info.UpdatablePointStructureObject.Equals(iUpdatablePointStructure)) == null)
                {
                    PointableObjectInfo pointableObjectInfo =
                        new PointableObjectInfo(iUpdatablePointStructure, editorLevelObject.transform);

                    GeneratePoints(pointableObjectInfo);

                    activePointable = pointableObjectInfo;
                }
            }

            foreach (List<ShapePointInfo> shapePointInfos in shapePointInfosByIPointable.Values)
            {
                foreach (ShapePointInfo shapePointInfo in shapePointInfos)
                {
                    shapePointInfo.pointHandler.SelectedObjects = selectedObjects;
                }
            }
        }


        public override void UpdateController(float deltaTime, SelectedObjectHandle.State state,
            Vector3 mousePosition, Vector3 lastMousePosition)
        {
            base.UpdateController(deltaTime, state, mousePosition, lastMousePosition);

            switch (state)
            {
                case SelectedObjectHandle.State.SingleObjectSelected:
                    UpdateDrag(mousePosition, lastMousePosition);
                    break;

                case SelectedObjectHandle.State.MultipleObjectsSelected:
                    UpdateDrag(mousePosition, lastMousePosition);
                    break;
                case SelectedObjectHandle.State.Selecting:
                    break;

                case SelectedObjectHandle.State.SetttingLink:
                    break;
                default:
                    UpdateDrag(mousePosition, lastMousePosition);
                    break;
            }
        }


        protected override void SetHandlersSnapping(float snapSize)
        {
            base.SetHandlersSnapping(snapSize);

            foreach (List<ShapePointInfo> shapePointInfos in shapePointInfosByIPointable.Values)
            {
                foreach (ShapePointInfo shapePointInfo in shapePointInfos)
                {
                    shapePointInfo.pointHandler.SetSnapSize(snapSize);
                }
            }
        }

        #endregion
    }
}
