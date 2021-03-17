using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters.Utils.Ui
{
    public class ScrollRectButtonsHolder
    {
        #region Fields

        private readonly IScrollHelper scrollHelper;

        private readonly IScrollButton[] scrollButtons;

        private RectTransform startDragRectTransform;
        private Rect startDragRect;

        #endregion



        #region Class lifecycle

        public ScrollRectButtonsHolder(IScrollHelper _rubberScrollHelper, IScrollButton[] _scrollButtons)
        {
            scrollHelper = _rubberScrollHelper;
            scrollButtons = _scrollButtons;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            scrollHelper.AddCallback(OnDragScrollStarted, EventTriggerType.BeginDrag);
        }


        public void Deinitialize()
        {
            scrollHelper.RemoveCallback(OnDragScrollStarted, EventTriggerType.BeginDrag);
            scrollHelper.RemoveCallback(OnDragScroll, EventTriggerType.Drag);
            scrollHelper.RemoveCallback(OnDragScrollFinished, EventTriggerType.EndDrag);
        }


        private void OnDragScrollStarted(BaseEventData args)
        {
            PointerEventData pointerEventData = args as PointerEventData;

            IScrollButton foundButton = scrollButtons.Find(e => e.ButtonRectTransform.gameObject == pointerEventData.selectedObject);

            if (foundButton != null)
            {
                startDragRectTransform = foundButton.ButtonRectTransform;
                startDragRect = foundButton.GetButtonWorldRect(scrollHelper.MainCanvas.transform.localScale);

                scrollHelper.AddCallback(OnDragScroll, EventTriggerType.Drag);
                scrollHelper.AddCallback(OnDragScrollFinished, EventTriggerType.EndDrag);
            }
        }


        private void OnDragScroll(BaseEventData args)
        {
            PointerEventData pointerEventData = args as PointerEventData;

            Vector3 pos = startDragRectTransform.InverseTransformPoint(pointerEventData.pointerCurrentRaycast.worldPosition);
            bool isInButtonRect = IsButtonInRect(pointerEventData, startDragRect);

            if (!isInButtonRect)
            {
                VisitAllElements(e => e.OnPointerUp(pointerEventData));
            }
        }


        private void OnDragScrollFinished(BaseEventData args)
        {
            scrollHelper.RemoveCallback(OnDragScroll, EventTriggerType.Drag);
            scrollHelper.RemoveCallback(OnDragScrollFinished, EventTriggerType.EndDrag);

            PointerEventData pointerEventData = args as PointerEventData;
            bool wasInButtonRect = IsButtonInRect(pointerEventData, startDragRect);

            VisitAllElements((e) =>
            {
                if (wasInButtonRect)
                {
                    e.OnShouldClick(pointerEventData);
                }

                e.OnPointerUp(pointerEventData);
            });
        }


        private bool IsButtonInRect(PointerEventData pointerEventData, Rect rect) =>
            rect.Contains(pointerEventData.pointerCurrentRaycast.worldPosition);


        private void VisitAllElements(Action<IScrollButton> callback)
        {
            foreach (var scrollButton in scrollButtons)
            {
                callback?.Invoke(scrollButton);
            }
        }

        #endregion
    }
}
