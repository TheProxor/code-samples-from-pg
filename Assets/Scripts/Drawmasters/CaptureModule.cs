using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class CaptureModule<T> where T : Component
    {
        #region Fields

        public event Action<T, Vector2> OnCapture;

        private readonly Camera camera;
        private readonly LayerMask layerMask;

        private bool isDraging;

        #endregion



        #region Fields

        public bool IsCaptured => Current != null;

        public T Current { get; private set; }

        #endregion



        #region Class lifecycle

        public CaptureModule(Camera _camera, LayerMask _layerMask)
        {
            camera = _camera;
            layerMask = _layerMask;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            TouchManager.OnBegan += InputTouch_OnBegan;
            TouchManager.OnMove += InputTouch_OnMove;
            TouchManager.OnEnded += InputTouch_OnEnded;

            isDraging = false;
        }


        public void Deinitialize()
        {
            TouchManager.OnBegan -= InputTouch_OnBegan;
            TouchManager.OnMove -= InputTouch_OnMove;
            TouchManager.OnEnded -= InputTouch_OnEnded;

            ResetCapture();
            isDraging = false;
        }


        public void ResetDrag() =>
            isDraging = false;


        public void ResetCapture() =>
            Current = null;


        private void RaycastObject(Vector2 touchPosition)
        {
            if (Current != null)
            {
                return;
            }
            
            Ray ray = camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, layerMask);

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    T shooter = hit.collider.GetComponent<T>();

                    if (shooter != null)
                    {
                        Current = shooter;

                        OnCapture?.Invoke(shooter, touchPosition);
                    }
                }
            }
        }

        #endregion



        #region Events handlers

        private void InputTouch_OnBegan(Vector2 touchPosition)
        {
            RaycastObject(touchPosition);
            isDraging = true;
        }


        private void InputTouch_OnMove(Vector2 touchPosition)
        {
            if (isDraging && !IsCaptured)
            {
                RaycastObject(touchPosition);
            }
        }


        private void InputTouch_OnEnded(bool success, Vector2 touchPosition) =>
            ResetDrag();

        #endregion
    }
}
