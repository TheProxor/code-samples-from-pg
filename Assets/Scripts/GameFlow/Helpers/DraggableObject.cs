using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters
{
    public class DraggableObject : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        #region Fields

        public event Action<DraggableObject> OnDragged;

        public event Action OnEndDragging;

        private Camera mainCamera;

        #endregion



        #region Public methods

        public void SetupCamera(Camera _mainCamera) => mainCamera = _mainCamera;


        public void OnDrag(PointerEventData eventData)
        {
            transform.position = (mainCamera == null) ?
                                  Input.mousePosition : mainCamera.ScreenToWorldPoint(Input.mousePosition).SetZ(0.0f);

            OnDragged?.Invoke(this);
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragging?.Invoke();
        }

        #endregion
    }
}
