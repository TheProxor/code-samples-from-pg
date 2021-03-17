using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace Drawmasters.Utils.Ui
{    /// <summary>
     /// Most of logic are correct only for only vertical scroll direction.
     /// Editing this script, don't forget also to edit RubberScrollHelper.cs as there are not general interface for ScrollRect
     /// </summary>
    public class ScrollHelper : IScrollHelper
    {
        #region Fields

        private readonly ScrollRect scrollRect;
        private readonly EventTrigger eventTrigger;

        private readonly Canvas mainCanvas;
        private readonly Rect mainCanvasRect;

        #endregion



        #region Properties

        public Canvas MainCanvas => mainCanvas;

        public RectTransform ViewportRectTransform => scrollRect.viewport;

        public Vector3 ScrollTopPosition
        {
            get
            {
                float contentOffset = scrollRect.content.rect.height * (1 - scrollRect.content.pivot.y);

                float anchorY = (scrollRect.content.anchorMax.y + scrollRect.content.anchorMin.y) * 0.5f;
                float canvasOffset = mainCanvasRect.height * (1 - anchorY);

                float maxPositionY = contentOffset - canvasOffset;

                Vector3 result = scrollRect.content.anchoredPosition3D.SetY(-maxPositionY);

                return result;
            }
        }


        public Vector3 ScrollBottomPosition
        {
            get
            {
                Vector3 startPosition = ScrollTopPosition;

                float contentOffset = scrollRect.content.rect.height - mainCanvasRect.height;
                Vector3 result = startPosition.SetY(startPosition.y + contentOffset);

                return result;
            }
        }

        #endregion



        #region Class lifecycle

        public ScrollHelper(ScrollRect _scrollRect, Canvas _mainCanvas, EventTrigger _eventTrigger = null)
        {
            scrollRect = _scrollRect;
            eventTrigger = _eventTrigger;
            mainCanvas = _mainCanvas;

            mainCanvasRect = (mainCanvas.transform as RectTransform).rect;
        }

        #endregion



        #region Methods

        public void AddCallback(UnityAction<BaseEventData> callback, EventTriggerType eventTriggerType)
        {
            EventTrigger.Entry entry = FindTriggerEntry(eventTriggerType);

            if (entry != null)
            {
                entry.callback.AddListener(callback);
            }
        }


        public void RemoveCallback(UnityAction<BaseEventData> callback, EventTriggerType eventTriggerType)
        {
            EventTrigger.Entry entry = FindTriggerEntry(eventTriggerType);

            if (entry != null)
            {
                entry.callback.RemoveListener(callback);
            }
        }

        /// <summary>
        /// Return position for scroll content, that is centeral view for target rect transform
        /// </summary>
        public Vector3 GetCentredSnapPosition(RectTransform target)
        {
            Vector3 snap = GetSnapPosition(target);
            float positionY = snap.y + (mainCanvasRect.height * 0.5f);
            Vector3 result = snap.SetX(default).SetY(positionY).SetZ(default);

            return result;
        }


        public Vector3 GetSnapPosition(RectTransform target) =>
            scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position);


        public void Scroll(Vector3 beginPosition, Vector3 endPosition, VectorAnimation animation, Action callback = default)
        {
            scrollRect.enabled = false;
            scrollRect.content.anchoredPosition3D = beginPosition;

            animation.beginValue = beginPosition;
            animation.endValue = endPosition;

            animation.Play((value) => scrollRect.content.anchoredPosition3D = value, this, () =>
            {
                scrollRect.enabled = true;

                callback?.Invoke();
            });
        }


        public void ExpandContent(VerticalLayoutGroup centralLineLayout, Vector2 additionalSizeDelta)
        {
            RectTransform centralLineLayoutRectTransform = centralLineLayout.transform as RectTransform;
            float contentHeightY = centralLineLayoutRectTransform.rect.height;

            scrollRect.content.sizeDelta = scrollRect.content.sizeDelta.SetY(contentHeightY) + additionalSizeDelta;
        }


        public Vector3 GetElementViewPosition(RectTransform target) =>
            scrollRect.viewport.transform.InverseTransformPoint(target.position);



        public void AddOnValueChangedCallback(UnityAction<Vector2> callback) =>
            scrollRect.onValueChanged.AddListener(callback);

        public void RemoveOnValueChangedCallback(UnityAction<Vector2> callback) =>
            scrollRect.onValueChanged.RemoveListener(callback);


        private EventTrigger.Entry FindTriggerEntry(EventTriggerType eventTriggerType)
        {
            if (eventTrigger == null)
            {
                Debug.LogError($"Event Trigger was not setted");
                return default;
            }

            EventTrigger.Entry entry = eventTrigger.triggers.Find(e => e.eventID == eventTriggerType);

            if (entry == null)
            {
                Debug.LogError($"Can't find EventTriggerType {eventTriggerType} in {eventTrigger.triggers}");
            }

            return entry;
        }

        #endregion
    }
}
