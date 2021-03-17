using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class RotationHandlerController : BaseHandlersController
    {
        #region Nested types

        [Serializable]
        class KeyToAxis
        {
            public KeyCode key = default;
            public Vector3 axis = default;
        }

        #endregion



        #region Fields

        [SerializeField] private HandlerInfo rotationHandlerInfo = default;
        [SerializeField] private float rotationSpeed = default;

        [SerializeField] private List<KeyToAxis> keysToRotationAxes = null;

        [SerializeField] private KeyCode fastRotationKeyCode = KeyCode.G;
        [SerializeField] private float fastRotationAngle = 45f;

        private BaseHandler rotationHandler = null;

        private Vector3 axisToRotateAround;

        #endregion



        #region BaseHandlersController

        public override void Initialize(Camera editorCamera, ObjectsGridDrawer objectsGridDrawer)
        {
            base.Initialize(editorCamera, objectsGridDrawer);

            rotationHandler = Instantiate(handlerPrefab, transform);
            float snapSize = (objectsGridDrawer.IsGridAvailable) ? objectsGridDrawer.SnapSize : handlersSnapSize;
            rotationHandler.Initialize(rotationHandlerInfo.direction, rotationHandlerInfo.unSelectedColor,
                rotationHandlerInfo.selectedColor, snapSize, selectedObjects);
        }


        public override void UpdateController(float deltaTime, SelectedObjectHandle.State state,
            Vector3 mousePosition, Vector3 lastMousePosition)
        {
            base.UpdateController(deltaTime, state, mousePosition, lastMousePosition);

            switch (state)
            {
                case SelectedObjectHandle.State.SingleObjectSelected:

                    if (Input.GetKeyDown(fastRotationKeyCode))
                    {
                        foreach (EditorLevelObject selectedObject in selectedObjects)
                        {
                            selectedObject.StartRotating();

                            Vector3 rotationAngle = selectedObject.transform.eulerAngles + Vector3.zero.SetZ(fastRotationAngle);
                            selectedObject.transform.eulerAngles = rotationAngle;
                            selectedObject.FinishRotating();
                        }
                    }

                    foreach (var keyToAxis in keysToRotationAxes)
                    {
                        if (Input.GetKey(keyToAxis.key))
                        {
                            axisToRotateAround += keyToAxis.axis;
                        }
                    }

                    if (axisToRotateAround != Vector3.zero)
                    {
                        Vector3 rotationAngle = rotationSpeed * axisToRotateAround * Time.fixedDeltaTime;

                        foreach (EditorLevelObject selectedObject in selectedObjects)
                        {
                            selectedObject.StartRotating();
                            selectedObject.Rotate(rotationAngle);
                        }

                        axisToRotateAround = Vector3.zero;
                    }
                    else
                    {
                        foreach (EditorLevelObject selectedObject in selectedObjects)
                        {
                            selectedObject.FinishRotating();
                        }
                    }

                    UpdateDrag(mousePosition, lastMousePosition);
                    break;

                case SelectedObjectHandle.State.MultipleObjectsSelected:
                    UpdateDrag(mousePosition, lastMousePosition);

                    axisToRotateAround = Vector3.zero;
                    break;

                case SelectedObjectHandle.State.Selecting:
                    axisToRotateAround = Vector3.zero;
                    break;

                case SelectedObjectHandle.State.SetttingLink:
                    axisToRotateAround = Vector3.zero;
                    break;
                default:
                    UpdateDrag(mousePosition, lastMousePosition);
                    axisToRotateAround = Vector3.zero;
                    break;
            }

            rotationHandler.UpdateRelativePosition();
        }


        public override bool IsHandlerBelongsToController(BaseHandler baseHandler)
        {
            return rotationHandler.Equals(baseHandler);
        }


        public override void SetSelectedObject(List<EditorLevelObject> editorLevelObjects)
        {
            base.SetSelectedObject(editorLevelObjects);

            rotationHandler.SelectedObjects = editorLevelObjects;
        }


        protected override void UpdateAvailability(bool isAvailable)
        {
            base.UpdateAvailability(isAvailable);

            CommonUtility.SetObjectActive(rotationHandler.gameObject, isAvailable);
        }


        protected override void SetHandlersSnapping(float snapSize)
        {
            base.SetHandlersSnapping(snapSize);

            rotationHandler.SetSnapSize(snapSize);
        }

        #endregion
    }
}
