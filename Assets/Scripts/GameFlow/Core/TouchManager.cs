using System;
using System.Collections.Generic;
using Drawmasters.Ui;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters
{
    public class TouchManager : SingletonMonoBehaviour<TouchManager>, IInitializable
    {
        #region Fields

        public static event Action OnBeganTouchAnywhere;
        public static event Action OnEndTouchAnywhere;

        public static event Action<bool> OnEnable;
        public static event Action<Vector2> OnFirstTouch;
        public static event Action<Vector2> OnBegan;
        public static event Action<Vector2> OnMove;
        public static event Action<bool, Vector2> OnEnded;

        public static event Action<TouchPhase> OnTouchPhase;

#if UNITY_EDITOR
        private const int simulatedFingerId = 10;
        private const int leftMouseButton = 0;
#endif

        private bool isEnabled;

        private bool isBegan;
        private bool isBeganAnywhere;

        #endregion



        #region Properties

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value)
                {
                    return;
                }

                isEnabled = value;

                if (isEnabled)
                {
                    MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
                }
                else
                {

                    MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
#if UNITY_EDITOR
                        EndTouch(false, Input.mousePosition);
#endif
                }

                OnEnable?.Invoke(isEnabled);
            }
        }

        #endregion



        #region Methods

        public void Initialize()
        {
#if DEBUG_TARGET || UA_BUILD // to open UA meny with 4 simultaneously touches
                Input.multiTouchEnabled = true;
#else
            Input.multiTouchEnabled = false;
#endif

            IsEnabled = true;
        }


        private void HandleTouch(int touchFingerId, Vector3 touchPosition, TouchPhase touchPhase)
        {
            bool overGui = default;

#if UNITY_EDITOR
                overGui = EventSystem.current != null && EventSystemController.IsPointerOverGameObject();
#else
            overGui = EventSystem.current != null && EventSystemController.IsPointerOverGameObject(touchFingerId);
#endif
            //TODO HACK
            overGui |= !UiScreenManager.IsTouchable();
            
            switch (touchPhase)
            {
                case TouchPhase.Began:
                    if (EventSystem.current != null)
                    {
                        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = touchPosition };
                        List<RaycastResult> results = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                        overGui = results.Count > 0;
                        //TODO HACK
                        overGui |= !UiScreenManager.IsTouchable();
                    }

                    if (!overGui)
                    {
                        OnFirstTouch?.Invoke(touchPosition);
                        UpdateTouch(touchPosition);
                    }

                    if (!isBeganAnywhere)
                    {
                        isBeganAnywhere = true;
                        OnBeganTouchAnywhere?.Invoke();
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (!overGui)
                    {
                        UpdateTouch(touchPosition);
                    }
                    break;

                case TouchPhase.Ended:
                    EndTouch(!overGui, touchPosition);
                    EndAnywhereTouch();

                    break;

                case TouchPhase.Canceled:
                    EndTouch(false, touchPosition);
                    EndAnywhereTouch();
                    break;

                default:
                    break;
            }

            OnTouchPhase?.Invoke(touchPhase);
        }


        private void UpdateTouch(Vector3 position)
        {
            if (!isBegan)
            {
                isBegan = true;
                OnBegan?.Invoke(position);
            }
            else
            {
                OnMove?.Invoke(position);
            }
        }


        private void EndTouch(bool success, Vector2 position)
        {
            if (isBegan)
            {
                isBegan = false;
                OnEnded?.Invoke(success, position);
            }
        }


        private void EndAnywhereTouch()
        {
            if (isBeganAnywhere)
            {
                isBeganAnywhere = false;
                OnEndTouchAnywhere?.Invoke();
            }
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
#if UNITY_EDITOR
            // Simulate touch events from mouse events
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(leftMouseButton))
                {
                    HandleTouch(simulatedFingerId, Input.mousePosition, TouchPhase.Began);
                }
                else if (Input.GetMouseButton(leftMouseButton))
                {
                    HandleTouch(simulatedFingerId, Input.mousePosition, TouchPhase.Moved);
                }
                else
                {
                    bool overGui = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                    if (Input.GetMouseButtonUp(leftMouseButton) || overGui)
                    {
                        HandleTouch(simulatedFingerId, Input.mousePosition, TouchPhase.Ended);
                    }
                }
            }
#else
            // Handle native touch events
            foreach (Touch touch in Input.touches)
            {
                HandleTouch(touch.fingerId, touch.position, touch.phase);
            }
#endif
        }

        #endregion
    }
}
