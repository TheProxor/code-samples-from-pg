using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Drawmasters.Ui
{
    public class PointerHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region Events

        public event Action OnLeftSwipe;
        public event Action OnRightSwipe;

        #endregion



        #region Fields

        private const float swipeSensitive = 1.75f;

        private bool isDragging;
        private Vector2 beginDragPoint;
        private Vector2 beginPressPoint;

        #endregion



        #region IBeginDragHandler

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            beginDragPoint = eventData.pointerCurrentRaycast.screenPosition;
        }

        #endregion



        #region IDragHandler

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                Vector2 dragPoint = eventData.pointerCurrentRaycast.screenPosition;
                Vector2 dragged = dragPoint - beginDragPoint;

                bool isEnough = dragged.magnitude / Screen.dpi >= swipeSensitive;
                if (isEnough)
                {
                    if (dragged.x < 0)
                    {
                        OnLeftSwipe?.Invoke();

                        EndDragging();
                    }
                    else
                    {
                        OnRightSwipe?.Invoke();

                        EndDragging();
                    }
                }
                
            }
        }

        #endregion



        #region IEndDragHandler

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragging();
        }

        #endregion



        #region Methods

        private void EndDragging()
        {
            isDragging = false;
        }
        #endregion
    }
}

