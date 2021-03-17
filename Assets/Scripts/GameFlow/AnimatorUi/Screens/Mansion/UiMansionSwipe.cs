using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.General;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Drawmasters.Ui
{
    [RequireComponent(typeof(RectTransform), typeof(Mask), typeof(Image))]
    public class UiMansionSwipe : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region Helpers

        public enum SwipeType
        {
            Horizontal,
            Vertical
        }

        #endregion



        #region Events 

        public event Action<int> OnScreenChanged;

        public event Action OnSwipeBegin;

        public event Action OnSwipeEnd;

        public event Action<int, float> OnPageHid; // page nunmber, factor

        #endregion



        #region Fields

        [Header("Swipe")]
        [SerializeField] private SwipeType swipeType = SwipeType.Horizontal;

        [SerializeField]
        private float elasticRubberFactor = 0.1f;

        [SerializeField, Tooltip("Time a swipe must happen within (s)")]
        private float swipeTime = 0.5f;

        [SerializeField, Tooltip("Velocity required to change screen")]
        private int swipeVelocityThreshold = 50;

        [Header("Content")]
        [SerializeField, Tooltip("Will contents be masked?")]
        private bool maskContent = true;

        [SerializeField, Tooltip("Parent object which contain the screens")]
        private RectTransform content = null;

        [SerializeField] private int currentScreen = 0;

        [SerializeField, Tooltip("Distance between screens")]
        private float spacing = 20f;

        [SerializeField]
        private float dragSensitivity = 7f;

        [SerializeField] private List<RectTransform> screens = null;

        [Header("Controls (Optional)")]
        [Tooltip("True = Acts like a normal screenRect but with snapping\nFalse = Can only change screens with buttons or from another script")]
        public bool isInteractable = true;


        [Header("Tween")]
        [SerializeField, Tooltip("Length of the tween (s)")]
        private float tweenTime = 0.5f;

        [SerializeField] private RectTransform contentFollower = default;

        [SerializeField]
        private AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);

        private float startTime = 0f;
        private bool isSwipe = false;
        private bool isDragBegin = false;

        private Vector2 velocity;
        private Mask _mask = null;

        private Image _maskImage = null;


        // bounds
        private Bounds contentBounds;
        private Bounds viewBounds;

        // start positions
        private Vector2 pointerStartLocalCursor;
        private Vector2 dragStartPos;


        private Coroutine tweenPageCoroutine = null;

        #endregion



        #region Properties

        public RectTransform rectTransform => (RectTransform)transform;

        private Image MaskImage
        {
            get
            {
                if (_maskImage == null)
                    _maskImage = GetComponent<Image>();
                return _maskImage;
            }
        }

        private Mask Mask
        {
            get
            {
                if (_mask == null)
                    _mask = GetComponent<Mask>();
                return _mask;
            }
        }

        public int CurrentScreen => currentScreen;

        public RectTransform Content
        {
            get => content;
            set => content = value;
        }

        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                SetScreenPositionsAndContentWidth();
            }
        }


        public int ScreenCount => screens.Count;


        #endregion



        #region Public methods

        public void Initialize()
        {
            SetScreenPositionsAndContentWidth();
        }


        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Screen Mangement / Public API
        /// <summary>
        /// Sets the screens positions and calculates the contents size
        /// </summary>
        private void SetScreenPositionsAndContentWidth()
        {
            Vector2 screenSize = rectTransform.rect.size;

            if (!content) return;

            for (int i = 0; i < screens.Count; i++)
            {
                // pivot and anchors
                screens[i].pivot = screens[i].anchorMin = screens[i].anchorMax =
                    swipeType == SwipeType.Horizontal
                        ? new Vector2(0, 0.5f)
                        : new Vector2(0.5f, 0);

                // size
                screens[i].sizeDelta = screenSize;

                // scale
                screens[i].localScale = Vector3.one;

                // position
                screens[i].anchoredPosition = swipeType == SwipeType.Horizontal
                    ? new Vector2((screenSize.x * i) + (spacing * i), 0)
                    : new Vector2(0, (screenSize.y * i) + (spacing * i));
            }

            // set content anchors and pivot
            content.pivot = content.anchorMin = content.anchorMax =
                swipeType == SwipeType.Horizontal
                    ? new Vector2(0, 0.5f)
                    : new Vector2(0.5f, 0);

            // set content size
            content.sizeDelta = swipeType == SwipeType.Horizontal
                ? new Vector2((screenSize.x + spacing) * screens.Count - spacing, screenSize.y)
                : new Vector2(screenSize.x, (screenSize.y + spacing) * screens.Count - spacing);
        }


        /// <summary>
        /// Tweens to a specific screen
        /// </summary>
        /// <param name="screenNumber">Screen number to tween to</param>
        public void GoToScreen(int screenNumber)
        {
            if (IsWithinScreenCount(screenNumber))
            {
                // set current screen
                currentScreen = screenNumber;

                // tween screen
                if (gameObject.activeInHierarchy)
                {
                    // if tween is already in motion stop it and start new one
                    if (tweenPageCoroutine != null)
                        StopCoroutine(tweenPageCoroutine);

                    tweenPageCoroutine = StartCoroutine(TweenPage(screens[currentScreen]));
                }
            }
            else
                Debug.LogErrorFormat($"Invalid screen number '{screenNumber}'. Must be between 0 and {screens.Count - 1}");
        }

        /// <summary>
        /// Goes to the next screen
        /// </summary>
        public void GoToNextScreen()
        {
            if (IsWithinScreenCount(CurrentScreen + 1))
                GoToScreen(CurrentScreen + 1);
        }

        /// <summary>
        /// Goes to the previous screen
        /// </summary>
        public void GoToPreviousScreen()
        {
            if (IsWithinScreenCount(CurrentScreen - 1))
                GoToScreen(CurrentScreen - 1);
        }

        /// <summary>
        /// Is the index within the screen count
        /// </summary>
        /// <param name="index">Index to check if within the screen count</param>
        /// <returns>True if within the screen count</returns>
        private bool IsWithinScreenCount(int index)
        {
            return index >= 0 && index < screens.Count;
        }
        #endregion



        #region Swipe and Drag Controlls

        public void TriggerDragBegin()
        {
            OnSwipeBegin?.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !isInteractable)
            {
                return;
            }

            // get start data
            dragStartPos = eventData.position;
            startTime = Time.time;

            pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, eventData.pressEventCamera, out pointerStartLocalCursor);

            if (ValidateDragBegin(eventData))
            {
                // cancel the page tween
                if (tweenPageCoroutine != null)
                    StopCoroutine(tweenPageCoroutine);

                isDragBegin = true;

                OnSwipeBegin?.Invoke();
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragBegin)
            {
                OnBeginDrag(eventData);
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left || !isInteractable)
            {
                return;
            }

            DragContent(eventData);

            // validate swipe boolean
            isSwipe = SwipeValidator(eventData.position);
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !isInteractable || !isDragBegin)
            {
                return;
            }

            // validate screen change and sets current screen 
            ScreenChangeValidate();

            // got to screen
            GoToScreen(currentScreen);

            isDragBegin = false;
            isSwipe = false;
        }

        public bool ValidateDragBegin(PointerEventData eventData) => 
                                dragSensitivity < eventData.delta.magnitude;

        

        /// <summary>
        /// Validates whether or not a swipe was make.
        /// Reasons for failure is swipe timer expired, or threshold is not met
        /// </summary>
        /// <param name="currentPosition">Cursor position</param>
        /// <returns>True if valid swipe</returns>
        private bool SwipeValidator(Vector2 currentPosition)
        {
            // get velocity
            velocity = currentPosition - dragStartPos;

            // is within time
            bool isWithinTime = Time.time - startTime < swipeTime;

            // set to true if it is the swipe type we wanted
            bool isSwipeTypeAndEnoughVelocity;

            // get absolute values of both velocity axis
            var velX = Mathf.Abs(velocity.x);
            var velY = Mathf.Abs(velocity.y);

            if (swipeType == SwipeType.Horizontal)
                isSwipeTypeAndEnoughVelocity = velX > velY && velX > swipeVelocityThreshold;
            else
                isSwipeTypeAndEnoughVelocity = velY > velX && velY > swipeVelocityThreshold;

            // return true if both are true
            return isWithinTime && isSwipeTypeAndEnoughVelocity;
        }

        /// <summary>
        /// Validates whether or not a screen can be changed.
        /// Reasons for failure is we're at the end of the screens list
        /// </summary>
        private void ScreenChangeValidate()
        {
            if (!isSwipe) return;

            int newPageNo;

            if (swipeType == SwipeType.Horizontal)
            {
                // get direction of swipe
                var leftSwipe = velocity.x < 0;

                // assign new page number
                newPageNo = leftSwipe ? currentScreen + 1 : currentScreen - 1;
            }
            else
            {
                // get direction of swipe
                var upSwipe = velocity.y < 0;

                // assign new page number
                newPageNo = upSwipe ? currentScreen + 1 : currentScreen - 1;
            }

            // if valid pageNo then update current page and invoke event
            if (IsWithinScreenCount(newPageNo))
            {
                // change current page
                currentScreen = newPageNo;

                // invoke changed event
                OnScreenChanged?.Invoke(currentScreen);
            }
        }

        /// <summary>
        /// Tweens the contents position
        /// </summary>
        /// <param name="toRectTransform">RectTransform to tween to</param>
        private IEnumerator TweenPage(RectTransform toRectTransform)
        {
            Vector2 from = content.anchoredPosition;
            Vector2 pos = new Vector2();
            float t = 0;

            while (pos != -toRectTransform.anchoredPosition || t < 1)
            {
                pos = Vector2.Lerp(from, -toRectTransform.anchoredPosition, ease.Evaluate(t));
                t += Time.deltaTime / tweenTime;
                SetContentAnchoredPosition(pos);

                yield return null;
            }

            OnSwipeEnd?.Invoke();

            tweenPageCoroutine = null;
        }

        #endregion



        #region Functions From Unity ScrollRect

        /* Note:
         * Everything in this region I sourced from Unity's ScrollRect script and rejigged to work in this script
         * Sources from: https://bitbucket.org/Unity-Technologies/ui 
         * Folder path: UI/UnityEngine.UI/UI/Core/ScrollRect.cs
         */

        /// <summary>
        /// Wrapper function <see cref="ScrollRect.OnDrag(PointerEventData)"/>
        /// </summary>
        /// <param name="eventData"></param>
        private void DragContent(PointerEventData eventData)
        {
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateContentBounds();

            var pointerDelta = localCursor - pointerStartLocalCursor;
            Vector2 position = content.anchoredPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(pointerDelta);
            position += offset;

            if (offset.x != 0)
                position.x = position.x - RubberDelta(offset.x, viewBounds.size.x);
            if (offset.y != 0)
                position.y = position.y - RubberDelta(offset.y, viewBounds.size.y);

            SetContentAnchoredPosition(position);
        }

        /// <summary>
        /// Sets the contents anchored position
        /// </summary>
        /// <param name="position">Position to set the content to</param>
        private void SetContentAnchoredPosition(Vector2 position)
        {
            if (swipeType == SwipeType.Vertical)
                position.x = content.anchoredPosition.x;

            if (swipeType == SwipeType.Horizontal)
                position.y = content.anchoredPosition.y;

            for (int i = 0; i < screens.Count; i++)
            {
                RectTransform tr = screens[i];

                float y = Mathf.Abs(position.y) - i * Mathf.Abs(tr.rect.height); // if pages have the same height

                float factor = Mathf.Clamp01(y / tr.rect.height);

                OnPageHid?.Invoke(i, factor);
            }

            if (position != content.anchoredPosition)
            {
                content.anchoredPosition = position;
                contentFollower.anchoredPosition = position;

                UpdateContentBounds();
            }
        }

        /// <summary>
        /// Updates the bounds of the scroll view content
        /// </summary>
        private void UpdateContentBounds()
        {
            viewBounds = new Bounds(rectTransform.rect.center, rectTransform.rect.size);
            contentBounds = GetContentBounds();

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = contentBounds.size;
            Vector3 contentPos = contentBounds.center;
            Vector3 excess = viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (content.pivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }

            contentBounds.size = contentSize;
            contentBounds.center = contentPos;
        }

        /// <summary>
        /// Gets the bounds of the scroll view content
        /// </summary>
        /// <returns>Content Bounds</returns>
        private Bounds GetContentBounds()
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = rectTransform.worldToLocalMatrix;

            Vector3[] m_Corners = new Vector3[4];
            content.GetWorldCorners(m_Corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        /// <summary>
        /// Offset to get content into place in the view.
        /// </summary>
        /// <param name="pointerDelta">Pointer delta</param>
        /// <returns>Offset to get content into place in the view.</returns>
        private Vector2 CalculateOffset(Vector2 pointerDelta)
        {
            Vector2 offset = Vector2.zero;

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            if (swipeType == SwipeType.Horizontal)
            {
                min.x += pointerDelta.x;
                max.x += pointerDelta.x;
                if (min.x > viewBounds.min.x)
                    offset.x = viewBounds.min.x - min.x;
                else if (max.x < viewBounds.max.x)
                    offset.x = viewBounds.max.x - max.x;
            }
            else
            {
                min.y += pointerDelta.y;
                max.y += pointerDelta.y;
                if (max.y < viewBounds.max.y)
                    offset.y = viewBounds.max.y - max.y;
                else if (min.y > viewBounds.min.y)
                    offset.y = viewBounds.min.y - min.y;
            }

            return offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="overStretching"></param>
        /// <param name="viewSize"></param>
        /// <returns></returns>
        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * elasticRubberFactor / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }
        #endregion



        #region Editor

        private void Reset()
        {
            maskContent = true;
            content = transform.GetChild(0) as RectTransform;
            swipeTime = 0.5f;
            swipeVelocityThreshold = 50;
            spacing = 20;
        }


        private void OnValidate()
        {
            Mask.showMaskGraphic = false;
            Mask.enabled = maskContent;
            MaskImage.enabled = maskContent;
        }

        #endregion
    }
}