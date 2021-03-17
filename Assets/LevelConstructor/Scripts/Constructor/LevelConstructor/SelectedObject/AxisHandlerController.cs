using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class AxisHandlerController : BaseHandlersController
    {
        #region Fields

        [SerializeField] private List<HandlerInfo> axisInfos = null;

        private List<BaseHandler> axisHandlers = new List<BaseHandler>();

        #endregion



        #region BaseHandlersController

        public override void Initialize(Camera editorCamera, ObjectsGridDrawer objectsGridDrawer)
        {
            base.Initialize(editorCamera, objectsGridDrawer);
            
            float snapSize = (objectsGridDrawer.IsGridAvailable) ? objectsGridDrawer.SnapSize : handlersSnapSize;
            foreach (var info in axisInfos)
            {
                BaseHandler handler = Instantiate(handlerPrefab, transform);
                handler.Initialize(info.direction, info.unSelectedColor, info.selectedColor, snapSize, selectedObjects);
                CommonUtility.SetObjectActive(handler.gameObject, false);
                axisHandlers.Add(handler);
            }
        }


        public override bool IsHandlerBelongsToController(BaseHandler baseHandler) => axisHandlers.Contains(baseHandler);


        public override void ChangeActiveHandlers(BaseHandler activeHandler)
        {
            base.ChangeActiveHandlers(activeHandler);
            
            foreach (BaseHandler axisHandler in axisHandlers)
            {
                axisHandler.ChangeSelection(axisHandler.Equals(activeHandler));
            }
        }


        public override void UpdateController(float deltaTime, 
                                              SelectedObjectHandle.State state, 
                                              Vector3 mousePosition, 
                                              Vector3 lastMousePosition)
        {
            base.UpdateController(deltaTime, state, mousePosition, lastMousePosition);

            if (EventSystemController.IsPointerOverGameObject())
            {
                return;
            }

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

            foreach (BaseHandler axisHandler in axisHandlers)
            {
                axisHandler.UpdateRelativePosition();
            }
        }


        public override void SetSelectedObject(List<EditorLevelObject> editorLevelObjects)
        {
            base.SetSelectedObject(editorLevelObjects);

            foreach (BaseHandler axisHandler in axisHandlers)
            {
                axisHandler.SelectedObjects = editorLevelObjects;
            }
        }


        protected override void UpdateAvailability(bool isAvailable)
        {
            foreach (BaseHandler axisHandler in axisHandlers)
            {
                CommonUtility.SetObjectActive(axisHandler.gameObject, isAvailable);
            }
        }


        protected override void SetHandlersSnapping(float snapSize)
        {
            base.SetHandlersSnapping(snapSize);
            
            foreach (BaseHandler axisHandler in axisHandlers)
            {
                axisHandler.SetSnapSize(snapSize);
            }
        }

        #endregion
    }
}
