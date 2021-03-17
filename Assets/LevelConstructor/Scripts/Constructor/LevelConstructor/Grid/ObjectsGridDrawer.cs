using Core;
using Drawmasters.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class ObjectsGridDrawer : MonoBehaviour
    {
        #region Fields

        public event Action<bool> OnGridAvailabilityChange;

        private const int AdditionalCellsBeforeCamera = 1;
        private const int GridObjectsPreInstantiateCount = 10;

        [SerializeField] private float cellSize = default;
        [SerializeField] private float cellGap = default;
        [SerializeField] private Camera editorCamera = default;
        [SerializeField] private GameCameraBordersDrawer gameCameraBordersDrawer = default;
        [SerializeField] private KeyCode gridAvailabilityKeyCode = default;
        [SerializeField] private GridObject gridObjectComponentPrefab = default;

        private bool isGridAvailable;
        private List<Vector2> gridCellsCenterPoint;
        private ComponentPool gridObjectsPool;

        private readonly List<GridObject> gridObjects = new List<GridObject>();
        private readonly List<GridObject> activeGridObjects = new List<GridObject>();

        #endregion



        #region Properties

        public float SnapSize => cellSize;


        public Vector3 ActiveGridObjectPosition
        {
            get
            {
                Vector3 result = Vector3.zero;

                if (activeGridObjects.Count > 0)
                {
                    result = activeGridObjects[0].transform.position;
                }
                else
                {
                    CustomDebug.LogError("Empty list of active grid objects");
                }

                return result;
            }
        }


        public bool IsGridAvailable
        {
            get => isGridAvailable;
            set
            {
                if (isGridAvailable != value)
                {
                    isGridAvailable = value;

                    ChangeGridAvailability(isGridAvailable);
                }
            }
        }


        private List<Vector2> GridCellsCenterPoint
        {
            get
            {
                if (gridCellsCenterPoint == null)
                {
                    gridCellsCenterPoint = GetGridCellsCenterPosition(cellSize, cellGap,
                        gameCameraBordersDrawer.GameCamera.transform.position, gameCameraBordersDrawer.LeftCameraMaxPositionX,
                        gameCameraBordersDrawer.RightCameraMaxPositionX, gameCameraBordersDrawer.TopCameraMaxPositionY,
                        gameCameraBordersDrawer.BottomCameraMaxPositionY);
                }

                return gridCellsCenterPoint;
            }
        }


        public ComponentPool GridObjectsPool
        {
            get
            {
                if (gridObjectsPool == null)
                {
                    gridObjectsPool = PoolManager.Instance.GetComponentPool(gridObjectComponentPrefab, true, GridObjectsPreInstantiateCount);
                }

                return gridObjectsPool;
            }
        }

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            InputKeys.EventInputKeyDown.Subscribe(gridAvailabilityKeyCode, GridAvailabilityKey_OnDown);
            GridObject.OnObjectActivityChange += GridObject_OnObjectActivityChange;
        }


        private void OnDisable()
        {
            InputKeys.EventInputKeyDown.Unsubscribe(gridAvailabilityKeyCode, GridAvailabilityKey_OnDown);
            GridObject.OnObjectActivityChange -= GridObject_OnObjectActivityChange;
        }

        #endregion



        #region Methods

        public Vector2 GetNearestPoint(Vector2 position)
        {
            Vector2 result = Vector3.zero;

            float distance = default;
            float minDistance = float.MaxValue;

            if (activeGridObjects.Count > 0)
            {
                foreach (GridObject activeGridObject in activeGridObjects)
                {
                    distance = Vector2.Distance(activeGridObject.transform.position.ToVector2(), position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result = activeGridObject.transform.position.ToVector2();
                    }
                }
            }
            else
            {
                foreach (GridObject gridObject in gridObjects)
                {
                    distance = Vector2.Distance(gridObject.transform.position.ToVector2(), position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result = gridObject.transform.position.ToVector2();
                    }
                }
            }

            return result;
        }


        private void GenerateGrid()
        {
            foreach (Vector2 point in GridCellsCenterPoint)
            {
                GridObject gridObject = GridObjectsPool.Pop() as GridObject;

                gridObject.gameObject.transform.SetParent(transform);

                if (gridObject != null)
                {
                    gridObject.Initialize(editorCamera, point, cellSize);
                    gridObjects.Add(gridObject);
                }
                else
                {
                    CustomDebug.LogWarning($"Missing grid object component for {gameObject.name}");
                }
            }
        }


        private void ChangeGridAvailability(bool isAvailable)
        {
            if (isAvailable)
            {
                if (gridObjects.Count <= 0)
                {
                    GenerateGrid();
                }
            }
            else
            {
                activeGridObjects.Clear();
            }

            foreach (GridObject gridObject in gridObjects)
            {
                CommonUtility.SetObjectActive(gridObject.gameObject, isAvailable);
            }

            OnGridAvailabilityChange?.Invoke(isAvailable);
        }


        private List<Vector2> GetGridCellsCenterPosition(float cellSize, float cellOffset, Vector3 initialPosition,
            float leftBorderX, float rightBorderX, float topBorderY, float bottomBorderY)
        {
            List<Vector2> result = new List<Vector2>();

            Vector2 initialPoint = initialPosition;
            int leftSizeCellsCount = Mathf.FloorToInt(leftBorderX / cellSize) - AdditionalCellsBeforeCamera;
            int rightSizeCellsCount = Mathf.CeilToInt(rightBorderX / cellSize) + AdditionalCellsBeforeCamera;
            int topSizeCellsCount = Mathf.CeilToInt(topBorderY / cellSize) + AdditionalCellsBeforeCamera;
            int bottomSizeCellsCount = Mathf.FloorToInt(bottomBorderY / cellSize) - AdditionalCellsBeforeCamera;

            Vector2 point = Vector2.zero;
            for (int i = leftSizeCellsCount; i <= rightSizeCellsCount; i++)
            {
                for (int j = bottomSizeCellsCount; j <= topSizeCellsCount; j++)
                {
                    point = new Vector2(initialPoint.x + i * (cellSize + cellOffset),
                        initialPoint.y + j * (cellSize + cellOffset));
                    result.Add(point);
                }
            }

            return result;
        }

        #endregion



        #region Events handlers

        private void GridAvailabilityKey_OnDown()
        {
            IsGridAvailable = !IsGridAvailable;
        }


        private void GridObject_OnObjectActivityChange(GridObject gridObject, bool isActive)
        {
            if (isActive)
            {
                if (!activeGridObjects.Contains(gridObject))
                {
                    activeGridObjects.Add(gridObject);
                }
            }
            else
            {
                activeGridObjects.Remove(gridObject);
            }
        }

        #endregion
    }
}

