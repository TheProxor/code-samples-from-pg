using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class DragHandlersController : BaseHandlersController
    {
        #region Fields

        [SerializeField] private HandlerInfo handlerInfo = default;

        private DragHandler dragHandler = default;

        #endregion



        #region Methods

        public override void Initialize(Camera editorCamera, ObjectsGridDrawer objectsGridDrawer)
        {
            base.Initialize(editorCamera, objectsGridDrawer);

            BaseHandler baseHandler = Instantiate(handlerPrefab, transform);
            dragHandler = baseHandler.GetComponent<DragHandler>();

            if (dragHandler != null)
            {
                float snapSize = (objectsGridDrawer.IsGridAvailable) ? objectsGridDrawer.SnapSize : handlersSnapSize;
                dragHandler.Initialize(handlerInfo.direction, handlerInfo.unSelectedColor, 
                    handlerInfo.selectedColor, snapSize, selectedObjects);
            }

            IsShowing = true;
        }


        public override void UpdateController(float deltaTime, SelectedObjectHandle.State state, 
            Vector3 mousePosition, Vector3 lastMousePosition)
        {
            base.UpdateController(deltaTime, state, mousePosition, lastMousePosition);

            mousePosition = activeCamera.ScreenToWorldPoint(mousePosition);
            lastMousePosition = activeCamera.ScreenToWorldPoint(lastMousePosition);

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
            
            dragHandler.UpdateRelativePosition();
        }


        public override bool IsHandlerBelongsToController(BaseHandler baseHandler)
        {
            return dragHandler.Equals(baseHandler);
        }


        public override void SetSelectedObject(List<EditorLevelObject> editorLevelObjects)
        {
            base.SetSelectedObject(editorLevelObjects);

            dragHandler.SelectedObjects = editorLevelObjects;
        }


        protected override void UpdateAvailability(bool isAvailable)
        {
            base.UpdateAvailability(isAvailable);
            
            CommonUtility.SetObjectActive(dragHandler.gameObject, isAvailable);
        }


        protected override void SetHandlersSnapping(float snapSize)
        {
            base.SetHandlersSnapping(snapSize);
            
            dragHandler.SetSnapSize(snapSize);
        }

        #endregion
    }
}
