using System.Collections.Generic;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class ObjectsCopier : MonoBehaviour
    {
        #region Nested types

        class SaveData
        {
            public Vector2 objectsCenter = Vector3.zero;
            public List<LevelObjectData> objectsData = new List<LevelObjectData>();
        }

        #endregion



        #region Fields

        private const float DistanceDenominator = 0.25f;

        [SerializeField] private Camera editorCamera = null;
        [SerializeField] private EditorLevel level = null;
        [SerializeField] private SelectedObjectHandle selectedObjectHandle = null;
        [Header("Keys")]
        [SerializeField] private KeyCode keyToPressToCopy = KeyCode.None;
        [SerializeField] private KeyCode keyToHoldToCopy = KeyCode.None;
        [SerializeField] private KeyCode keyToPressToPaste = KeyCode.None;
        [SerializeField] private KeyCode keyToHoldToPaste = KeyCode.None;

        [SerializeField] private KeyCode keyToPressToDuplicate = KeyCode.None;
        [SerializeField] private KeyCode keyToHoldToDuplicate = KeyCode.None;

        [SerializeField] private KeyCode keyToHoldToDuplicateWithAxisMove = KeyCode.None;

        [SerializeField] private ObjectsGridDrawer objectsGridDrawer = default;

        private static SaveData saveData;

        private List<EditorLevelObject> selectedObjects;

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            SelectedObjectChange.Subscribe(SetupSelectedObjects);
        }


        private void OnDisable()
        {
            SelectedObjectChange.Unsubscribe(SetupSelectedObjects);
            BaseHandlersController.OnDragUpdated -= AxisHandlerController_OnDragUpdated;
        }

        private void Update()
        {
            if (Input.GetKey(keyToHoldToCopy) && Input.GetKeyDown(keyToPressToCopy))
            {
                Copy();
            }

            if (Input.GetKey(keyToHoldToPaste) && Input.GetKeyDown(keyToPressToPaste))
            {
                Paste();
            }

            if (Input.GetKey(keyToHoldToDuplicate) && Input.GetKeyDown(keyToPressToDuplicate))
            {
                Copy();
                Paste(true);
            }

            if (Input.GetKeyDown(keyToHoldToDuplicateWithAxisMove))
            {
                BaseHandlersController.OnDragUpdated += AxisHandlerController_OnDragUpdated;
            }
            else if (Input.GetKeyUp(keyToHoldToDuplicateWithAxisMove))
            {
                BaseHandlersController.OnDragUpdated -= AxisHandlerController_OnDragUpdated;
            }
        }



        private void Copy()
        {
            saveData = new SaveData();

            Vector2 minPosition = new Vector3(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector3(float.MinValue, float.MinValue);

            foreach (EditorLevelObject levelObject in selectedObjects)
            {
                LevelObjectData data = levelObject.GetData();

                minPosition = new Vector2(Mathf.Min(minPosition.x, data.position.x), Mathf.Min(minPosition.y, data.position.y));
                maxPosition = new Vector2(Mathf.Max(maxPosition.x, data.position.x), Mathf.Max(maxPosition.y, data.position.y));

                saveData.objectsData.Add(data);
            }

            saveData.objectsCenter = (minPosition + maxPosition) / 2;
        }


        private void Paste(bool matchOriginalObjectPosition = false)
        {
            if (saveData == null)
            {
                return;
            }

            Vector2 cursorPosition = editorCamera.ScreenToWorldPoint(Input.mousePosition.SetZ(-editorCamera.transform.position.z));
            List<EditorLevelObject> createdObjects = new List<EditorLevelObject>();

            for (int i = 0; i < saveData.objectsData.Count; i++)
            {
                LevelObjectData data = saveData.objectsData[i].Copy();

                if (!matchOriginalObjectPosition)
                {
                    if (objectsGridDrawer.IsGridAvailable)
                    {
                        data.position = objectsGridDrawer.ActiveGridObjectPosition;

                    }
                    else
                    {
                        data.position -= (saveData.objectsCenter - cursorPosition).Snap(DistanceDenominator).ToVector3();
                    }
                }

                EditorLevelObject createdObject = level.SpawnNewObject(data.index, data);

                createdObjects.Add(createdObject);
            }

            BaseHandlersController activeHandler = selectedObjectHandle.ActiveHandlersController;

            selectedObjectHandle.ChangeSelection(createdObjects);
            selectedObjectHandle.ActiveHandlersController = activeHandler;
        }

        #endregion



        #region Events handlers

        private void SetupSelectedObjects(List<EditorLevelObject> selectedObjects)
        {
            this.selectedObjects = selectedObjects;
        }


        private void AxisHandlerController_OnDragUpdated()
        {
            if (Input.GetKey(keyToHoldToDuplicateWithAxisMove))
            {
                Copy();
                Paste(true);

                BaseHandlersController.OnDragUpdated -= AxisHandlerController_OnDragUpdated;
            }
        }

        #endregion
    }
}
