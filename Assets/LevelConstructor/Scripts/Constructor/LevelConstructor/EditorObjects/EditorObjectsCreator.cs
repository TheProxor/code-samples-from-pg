using Drawmasters.Levels;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorObjectsCreator : MonoBehaviour
    {
        #region Fields

        const float CreatedObjectPositionDenominator = 0.25f;

        [SerializeField] private EditorLevel level = default;
        [SerializeField] private Camera editorCamera = default;
        [SerializeField] private KeyCode creationKey = default;
        [SerializeField] private ObjectsGridDrawer objectsGridDrawer = default;

        private int selectedLibraryObjectIndex = -1;

        #endregion



        #region Unity Lifecycle

        private void Awake()
        {
            LibraryMenu.OnObjectSelected += ((int index) => selectedLibraryObjectIndex = index);
        }


        private void Update()
        {
            if (Input.GetKeyDown(creationKey))
            {
                CreateObject(); 
            }
        }

        #endregion



        #region Methods

        private void CreateObject()
        {
            Vector3 objectPosition;

            if (objectsGridDrawer.IsGridAvailable)
            {
                objectPosition = objectsGridDrawer.ActiveGridObjectPosition;

            }
            else
            {
                Vector3 mouseCameraWorldPosition = editorCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y, -editorCamera.transform.position.z));
                objectPosition = mouseCameraWorldPosition.Snap(CreatedObjectPositionDenominator);
            }

            LevelObjectData editorObjectData = new LevelObjectData(selectedLibraryObjectIndex, 
                                                                   objectPosition, 
                                                                   Vector3.zero, 
                                                                   true, 
                                                                   new LevelObjectMoveSettings(), 
                                                                   new List<Vector3>(),
                                                                   string.Empty);
                                                                   
            EditorLevelObject createdObject = level.SpawnNewObject(selectedLibraryObjectIndex, editorObjectData);

            if (createdObject != null)
            {
                createdObject.transform.position = objectPosition;
            }
        }

        #endregion
    }
}
