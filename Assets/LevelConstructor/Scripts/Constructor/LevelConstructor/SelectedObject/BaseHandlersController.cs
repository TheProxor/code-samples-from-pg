using Core;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class BaseHandlersController : MonoBehaviour
    {
        #region Fields

        public static event Action OnDragUpdated;
        public static event Action<BaseHandlersController> OnActivateBaseHandlersController;

        [SerializeField] protected BaseHandler handlerPrefab = default;
        [SerializeField] private KeyCode availabilityKey = default;

        [Header("Snapping")]
        [SerializeField] private KeyCode keyToSnap = default;
        [SerializeField] protected float handlersSnapSize = default;

        protected List<EditorLevelObject> selectedObjects = new List<EditorLevelObject>();
        protected Camera activeCamera;
        protected BaseHandler activeHandler;
        protected ObjectsGridDrawer objectsGridDrawer;

        private bool isShowing;
        private bool isKeySnappingActive;
        private bool isCustomSnappingActive;

        private float customSnapSize;

        private SelectedObjectHandle.State currentState = SelectedObjectHandle.State.Selecting;

        #endregion



        #region Properties

        protected bool IsShowing
        {
            get => isShowing;
            set
            {
                isShowing = value;
                UpdateAvailability(isShowing);

                if (isShowing)
                {
                    OnActivateBaseHandlersController?.Invoke(this);
                }
            }
        }


        private bool IsAnySnappingActive => IsCustomSnappingActive || IsKeySnappingActive;


        private bool IsCustomSnappingActive
        {
            get => isCustomSnappingActive;
            set
            {
                if (isCustomSnappingActive != value)
                {
                    isCustomSnappingActive = value;

                    if (isCustomSnappingActive)
                    {
                        SetHandlersSnapping(customSnapSize);
                    }
                }
            }
        }


        private bool IsKeySnappingActive
        {
            get => isKeySnappingActive;
            set
            {
                if (isKeySnappingActive != value)
                {
                    isKeySnappingActive = value;

                    if (isKeySnappingActive && !IsCustomSnappingActive)
                    {
                        SetHandlersSnapping(handlersSnapSize);
                    }
                }
            }
        }

        #endregion



        #region Unity lifecycle

        protected virtual void OnEnable()
        {
            InputKeys.EventInputKeyDown.Subscribe(availabilityKey, AxisAvailabilityKey_OnDown);
        }


        protected virtual void OnDisable()
        {
            InputKeys.EventInputKeyDown.Unsubscribe(availabilityKey, AxisAvailabilityKey_OnDown);
        }

        #endregion



        #region Methods

        public virtual void Initialize(Camera editorCamera, ObjectsGridDrawer objectsGridDrawer)
        {
            activeCamera = editorCamera;
            this.objectsGridDrawer = objectsGridDrawer;

            this.objectsGridDrawer.OnGridAvailabilityChange += ObjectsGridDrawer_OnGridAvailabilityChange;
        }


        public virtual void UpdateController(float deltaTime,
                                             SelectedObjectHandle.State state,
                                             Vector3 mousePosition,
                                             Vector3 lastMousePosition)
        {
            currentState = state;

            IsKeySnappingActive = Input.GetKey(keyToSnap);
        }


        public void HideHandler()
        {
            IsShowing = false;
        }


        public virtual void SetSelectedObject(List<EditorLevelObject> editorLevelObjects)
        {
            selectedObjects = editorLevelObjects;

            if (editorLevelObjects.Count > 0)
            {
                UpdateAvailability(IsShowing);
            }
            else
            {
                UpdateAvailability(false);
            }
        }


        public virtual bool IsHandlerBelongsToController(BaseHandler baseHandler)
        {
            return false;
        }


        public virtual void ChangeActiveHandlers(BaseHandler activeHandler)
        {
            if (this.activeHandler != null)
            {
                this.activeHandler.ChangeSelection(false);
            }

            this.activeHandler = activeHandler;

            if (activeHandler != null)
            {
                activeHandler.ChangeSelection(true);
            }
        }


        protected virtual void UpdateAvailability(bool isAvailable) { }


        protected virtual void SetHandlersSnapping(float snapSize) { }


        protected void UpdateDrag(Vector3 mousePosition, Vector3 lastMousePosition)
        {
            if (activeHandler != null &&
                Input.GetMouseButton(0))
            {
                if (mousePosition != lastMousePosition)
                {
                    activeHandler.Drag(mousePosition - lastMousePosition, activeCamera, IsAnySnappingActive);
                    OnDragUpdated?.Invoke();
                }
            }
        }


        private void SetCustomSnapping(bool isActive, float customSnapSize = default)
        {
            if (isActive)
            {
                this.customSnapSize = customSnapSize;
            }

            IsCustomSnappingActive = isActive;
        }

        #endregion



        #region Events handlers

        private void AxisAvailabilityKey_OnDown()
        {
            if (currentState != SelectedObjectHandle.State.Selecting)
            {
                IsShowing = !IsShowing;
            }
        }


        private void ObjectsGridDrawer_OnGridAvailabilityChange(bool isActive)
        {
            SetCustomSnapping(isActive, objectsGridDrawer.SnapSize);
        }

        #endregion
    }
}
