using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class SelectedObjectHandle : SerializedMonoBehaviour
    {
        #region Nested types

        public enum State
        {
            Selecting,
            SingleObjectSelected,
            MultipleObjectsSelected,
            SetttingLink
        }

        #endregion



        #region Fields

        [SerializeField]
        private Dictionary<HandlersControllerType, BaseHandlersController> handlersControllers =
            new Dictionary<HandlersControllerType, BaseHandlersController>();

        [SerializeField] private LayerMask levelObjectsLayer = default;
        [SerializeField] private LayerMask handlersLayer = default;
        [SerializeField] private Camera editorCamera = null;
        [SerializeField] private EditorLevel level = null;
        [SerializeField] private ObjectsGridDrawer objectsGridDrawer = default;

        [Header("Keys")]
        [SerializeField] private KeyCode multipleSelectionKey = KeyCode.None;
        [SerializeField] private KeyCode deletionKeyToHold = KeyCode.None;
        [SerializeField] private KeyCode deletionKeyToPress = KeyCode.None;
        [SerializeField] private KeyCode alternativeDeletionKey = KeyCode.None;
        [SerializeField] private KeyCode deselectionKey = KeyCode.None;

        private List<EditorLevelObject> selectedObjects = new List<EditorLevelObject>();
        private BaseHandlersController activeHandlersController = default;
        private List<BaseHandlersController> selectedObjectHandlersControllers = new List<BaseHandlersController>();

        private BaseHandler selectedHandle;

        private Vector2 lastMousePosition;

        private State currentState = State.Selecting;

        public static bool isSelectionLock;

        #endregion



        #region Properties

        public BaseHandlersController ActiveHandlersController
        {
            get => activeHandlersController;
            set
            {
                if (activeHandlersController != value)
                {
                    if (activeHandlersController != null)
                    {
                        activeHandlersController.ChangeActiveHandlers(null);
                    }

                    activeHandlersController = value;

                    if (activeHandlersController != null)
                    {
                        activeHandlersController.ChangeActiveHandlers(selectedHandle);
                    }
                }
            }
        }

        #endregion



        #region Unity Lifecycle

        private void Awake()
        {
            foreach (KeyValuePair<HandlersControllerType, BaseHandlersController> handlersController in handlersControllers)
            {
                handlersController.Value.Initialize(editorCamera, objectsGridDrawer);
            }

            SubscribeOnEvents();
        }


        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        private void SubscribeOnEvents()
        {
            BaseHandlersController.OnActivateBaseHandlersController += BaseHandlersController_OnActivateBaseHandlersController;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            LinkSettingRequest.Subscribe(OnLinkSettingRequest);
        }


        private void UnsubscribeFromEvents()
        {
            BaseHandlersController.OnActivateBaseHandlersController -= BaseHandlersController_OnActivateBaseHandlersController;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            LinkSettingRequest.Unsubscribe(OnLinkSettingRequest);
        }

        #endregion



        #region Methods

        public void SetSelectionLocked(bool locked)
        {
            if (isSelectionLock != locked)
            {
                isSelectionLock = locked;

                if (isSelectionLock)
                {
                    UnsubscribeFromEvents();
                }
                else
                {
                    SubscribeOnEvents();
                }
            }
        }


        public void ChangeSelection(List<EditorLevelObject> newSelectedObjects)
        {
            if (isSelectionLock)
            {
                return;
            }

            Deselect();
            newSelectedObjects.ForEach((selectedObject) => ChangeSelectedObject(selectedObject, true));
        }


        public void Deselect()
        {
            if (isSelectionLock)
            {
                return;
            }

            foreach (var selectedObject in selectedObjects)
            {
                selectedObject?.Deselect();
            }

            selectedObjects.Clear();
            ActiveHandlersController = null;

            foreach (KeyValuePair<HandlersControllerType, BaseHandlersController> handlersController in handlersControllers)
            {
                handlersController.Value.SetSelectedObject(selectedObjects);
            }

            currentState = State.Selecting;

            SelectedObjectChange.Register(selectedObjects);
        }


        private void UpdateSelection(Vector2 mousePosition)
        {
            bool isLeftClick = Input.GetMouseButtonDown(0);
            bool isPointerOverGameobject = EventSystemController.IsPointerOverGameObject();

            if (isLeftClick)
            {
                ActiveHandlersController = null;
            }

            if (isLeftClick &&
                !isPointerOverGameobject)
            {
                Ray ray = editorCamera.ScreenPointToRay(mousePosition, Camera.MonoOrStereoscopicEye.Mono);
                RaycastHit2D raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, handlersLayer);

                if (raycastHit2D.collider != null)
                {
                    selectedHandle = raycastHit2D.collider.GetComponent<BaseHandler>();
                    UpdateLevelObjectsActiveHandlersController(selectedHandle);

                    EditorLevelObject hitObject = raycastHit2D.collider.GetComponentInParent<EditorLevelObject>();
                    if (!hitObject.IsNull() && !selectedObjects.Contains(hitObject))
                    {
                        ChangeSelectedObject(hitObject, Input.GetKey(multipleSelectionKey));
                    }
                }
                else
                {
                    raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, levelObjectsLayer);

                    if (raycastHit2D.collider != null)
                    {
                        EditorLevelObject hitObject = raycastHit2D.collider.GetComponentInParent<EditorLevelObject>();

                        if (!hitObject.IsNull() && !selectedObjects.Contains(hitObject))
                        {
                            ChangeSelectedObject(hitObject, Input.GetKey(multipleSelectionKey));
                        }
                    }
                    else
                    {
                        Deselect();
                    }
                }
            }
        }


        private void TryDeleteObjects()
        {
            if ((Input.GetKey(deletionKeyToHold) && Input.GetKeyDown(deletionKeyToPress)) ||
                Input.GetKeyDown(alternativeDeletionKey))
            {
                selectedObjects.ForEach((selectedObject) => level.RemoveObject(selectedObject));
                Deselect();
            }
        }


        private void TryDeselectObjects()
        {
            if (Input.GetKeyDown(deselectionKey))
            {
                Deselect();
            }
        }


        private void UpdateLevelObjectsActiveHandlersController(BaseHandler baseHandler)
        {
            foreach (BaseHandlersController handlersController in selectedObjectHandlersControllers)
            {
                if (handlersController.IsHandlerBelongsToController(baseHandler))
                {
                    ActiveHandlersController = handlersController;
                }
            }
        }


        private void UpdateLevelObjectsControllers(float deltaTime,
                                                   State state,
                                                   Vector3 mousePosition,
                                                   Vector3 lastMousePosition)
        {
            // TODO: hotfix. Can't do the same using foreach cuz of throw collection modified exception after duplicating with "alt + axis move"
            for (int i = 0; i < selectedObjectHandlersControllers.Count; i++)
            {
                selectedObjectHandlersControllers[i].UpdateController(deltaTime, state, mousePosition, lastMousePosition);
            }
        }


        private void TrySetLink(Vector2 mousePosition)
        {
            if (Input.GetMouseButtonDown(0) &&
                !EventSystemController.IsPointerOverGameObject())
            {
                Ray ray = editorCamera.ScreenPointToRay(mousePosition, Camera.MonoOrStereoscopicEye.Mono);

                RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, levelObjectsLayer);

                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        EditorLevelObject hitObject = hit.collider.GetComponentInParent<EditorLevelObject>();

                        if (hitObject != selectedObjects[0])
                        {
                            selectedObjects[0].AddLink(hitObject);
                            break;
                        }
                    }
                }

                currentState = State.SingleObjectSelected;

                LinkSettingFinished.Register();
            }
        }


        private void ChangeSelectedObject(EditorLevelObject selectedObject, bool isAdded = false)
        {
            if (isAdded)
            {
                selectedObjects.Add(selectedObject);

                if (selectedObjects.Count > 1)
                {
                    currentState = State.MultipleObjectsSelected;
                }
                else
                {
                    currentState = State.SingleObjectSelected;
                }
            }
            else
            {
                Deselect();
                selectedObjects.Add(selectedObject);
                currentState = State.SingleObjectSelected;
            }

            selectedObject.Select();
            selectedObjectHandlersControllers.Clear();

            UpdateLevelObjectsActiveHandlersController(selectedHandle);

            foreach (HandlersControllerType controllerType in selectedObject.AvailableHandlers)
            {
                BaseHandlersController handlersController = null;
                handlersControllers.TryGetValue(controllerType, out handlersController);

                if (handlersController != null)
                {
                    handlersController.SetSelectedObject(selectedObjects);
                    selectedObjectHandlersControllers.Add(handlersController);
                }
                else
                {
                    CustomDebug.LogWarning($"Can't find HandlersController for type {controllerType}");
                }
            }

            SelectedObjectChange.Register(selectedObjects);
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            Vector2 mousePosition = Input.mousePosition;

            switch (currentState)
            {
                case State.SingleObjectSelected:
                case State.MultipleObjectsSelected:
                    UpdateSelection(mousePosition);
                    UpdateLevelObjectsControllers(deltaTime, currentState, mousePosition, lastMousePosition);

                    TryDeleteObjects();
                    TryDeselectObjects();
                    break;

                case State.Selecting:
                    UpdateSelection(mousePosition);
                    break;

                case State.SetttingLink:
                    TrySetLink(mousePosition);
                    break;
            }

            lastMousePosition = Input.mousePosition;
        }


        private void BaseHandlersController_OnActivateBaseHandlersController(BaseHandlersController activeHandldersController)
        {
            foreach (BaseHandlersController baseHandlersController in handlersControllers.Values)
            {
                if (!baseHandlersController.Equals(activeHandldersController))
                {
                    baseHandlersController.HideHandler();
                }
            }
        }



        private void OnLinkSettingRequest(bool isRequestStarted)
        {
            if (isRequestStarted)
            {
                if (currentState == State.SingleObjectSelected)
                {
                    currentState = State.SetttingLink;
                }
            }
            else
            {
                if (currentState == State.SetttingLink)
                {
                    switch (selectedObjects.Count)
                    {
                        case 0:
                            currentState = State.Selecting;
                            break;

                        case 1:
                            currentState = State.SingleObjectSelected;
                            break;

                        default:
                            currentState = State.MultipleObjectsSelected;
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
