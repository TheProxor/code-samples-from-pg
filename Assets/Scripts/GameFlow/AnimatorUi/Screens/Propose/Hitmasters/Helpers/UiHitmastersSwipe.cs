using System;
using System.Collections;
using System.Collections.Generic;
using Modules.General;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    [RequireComponent(typeof(RectTransform), typeof(Mask), typeof(Image))]
    public class UiHitmastersSwipe : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region Helpers

        public enum SwipeType
        {
            Horizontal,
            Vertical
        }

        #endregion


        
        #region Events

        public event Action<int> OnPointChanged;

        public event Action OnSwipeBegin;

        public event Action OnSwipeEnd;

        #endregion

        

        #region Fields

        [SerializeField] private RectTransform canvasTransform = default;

        [Header("Swipe")] [SerializeField] private SwipeType swipeType = SwipeType.Horizontal;

        [SerializeField, Tooltip("Time a swipe must happen within (s)")]
        private float swipeTime = 0.5f;

        [SerializeField, Tooltip("Velocity required to change screen")]
        private int swipeVelocityThreshold = 50;

        [SerializeField, Tooltip("Velocity required to change screen")]
        private bool swipeCentrScreen = true;
        
        [Header("Content")] [SerializeField, Tooltip("Will contents be masked?")]
        private bool maskContent = true;

        [SerializeField, Tooltip("Parent object which contain the screens")]
        private RectTransform content = null;

        [SerializeField] private int currentPoint = 0;

        [SerializeField] private List<RectTransform> points = null;

        [Header("Controls (Optional)")]
        [Tooltip(
            "True = Acts like a normal screenRect but with snapping\nFalse = Can only change screens with buttons or from another script")]
        public bool isInteractable = true;

        [Header("Tween")] [SerializeField, Tooltip("Length of the tween (s)")]
        public float tweenTime = 0.5f;

        [SerializeField] private RectTransform contentFollower = default;

        [SerializeField] private AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);

        private float startTime = 0f;
        private bool isSwipe = false;

        private Vector2 velocity;
        private Mask _mask = null;

        private Image _maskImage = null;
        
        // bounds
        private Bounds contentBounds;
        private Bounds viewBounds;

        // start positions
        private Vector2 pointerStartLocalCursor;
        private Vector2 dragStartPos;
        
        private Coroutine tweenPageCoroutine;

        #endregion

        

        #region Properties

        public RectTransform rectTransform => (RectTransform) transform;

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

        public int CurrentScreen => currentPoint;

        public RectTransform Content
        {
            get => content;
            set => content = value;
        }

        public int PointCount => points.Count;

        #endregion


        #region Public methods

        public void Initialize()
        {
        }


        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion


        #region Point Mangement / Public API

        /// <summary>
        /// Tweens to a specific point
        /// </summary>
        /// <param name="pointNumber">Screen number to tween to</param>
        public void GoToPoint(int pointNumber, Action callback = null)
        {
            if (IsWithinScreenCount(pointNumber))
            {
                // set current screen
                currentPoint = pointNumber;

                // tween screen
                if (gameObject.activeInHierarchy)
                {
                    // if tween is already in motion stop it and start new one
                    if (tweenPageCoroutine != null)
                    {
                        StopCoroutine(tweenPageCoroutine);
                    }
                    
                    float contentVisibleHight = content.sizeDelta.y - points[currentPoint].anchoredPosition.y;
                    Vector2 pointPosition = default;
                    float minHight = canvasTransform.sizeDelta.y / 2f;

                    if (contentVisibleHight < minHight)
                    {
                        pointPosition = new Vector2(points[currentPoint].anchoredPosition.x,
                            content.sizeDelta.y - canvasTransform.sizeDelta.y);
                    }
                    else if (swipeCentrScreen)
                    {
                        float x = points[currentPoint].anchoredPosition.y - minHight;
                        pointPosition = new Vector2(points[currentPoint].anchoredPosition.x, x > 0f ? x : 0f);
                    }
                    else
                    {
                        pointPosition = points[currentPoint].anchoredPosition;
                    }
                    tweenPageCoroutine = StartCoroutine(TweenPage(-pointPosition, callback));
                }
            }
            else
            {
                Debug.LogErrorFormat(
                    $"Invalid screen number '{pointNumber}'. Must be between 0 and {points.Count - 1}");
            }
        }

        /// <summary>
        /// Goes to the next screen
        /// </summary>
        public void GoToNextPoint()
        {
            if (IsWithinScreenCount(CurrentScreen + 1))
                GoToPoint(CurrentScreen + 1);
        }

        /// <summary>
        /// Goes to the previous screen
        /// </summary>
        public void GoToPreviousPoint()
        {
            if (IsWithinScreenCount(CurrentScreen - 1))
                GoToPoint(CurrentScreen - 1);
        }

        /// <summary>
        /// Is the index within the screen count
        /// </summary>
        /// <param name="index">Index to check if within the screen count</param>
        /// <returns>True if within the screen count</returns>
        private bool IsWithinScreenCount(int index)
        {
            return index >= 0 && index < points.Count;
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
                return;

            OnSwipeBegin?.Invoke();

            // cancel the page tween
            if (tweenPageCoroutine != null)
                StopCoroutine(tweenPageCoroutine);

            // get start data
            dragStartPos = eventData.position;
            startTime = Time.time;

            pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position,
                eventData.pressEventCamera, out pointerStartLocalCursor);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !isInteractable)
                return;

            DragContent(eventData);

            // validate swipe boolean
            isSwipe = SwipeValidator(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !isInteractable)
                return;

            // validate screen change and sets current screen 
            PointChangeValidate();

            // got to screen
            GoToPoint(currentPoint);

            isSwipe = false;
        }

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
        /// Reasons for failure is we're at the end of the points list
        /// </summary>
        private void PointChangeValidate()
        {
            if (!isSwipe) return;

            int newPageNo;

            if (swipeType == SwipeType.Horizontal)
            {
                // get direction of swipe
                var leftSwipe = velocity.x < 0;

                // assign new page number
                newPageNo = leftSwipe ? currentPoint + 1 : currentPoint - 1;
            }
            else
            {
                // get direction of swipe
                var upSwipe = velocity.y < 0;

                // assign new page number
                newPageNo = upSwipe ? currentPoint + 1 : currentPoint - 1;
            }

            // if valid pageNo then update current page and invoke event
            if (IsWithinScreenCount(newPageNo))
            {
                // change current page
                currentPoint = newPageNo;

                // invoke changed event
                OnPointChanged?.Invoke(currentPoint);
            }
        }
        
        /// <summary>
        /// Tweens the contents position
        /// </summary>
        /// <param name="toPos">Vector to tween to</param>
        private IEnumerator TweenPage(Vector2 toPos, Action callback = null)
        {
            Vector2 from = content.anchoredPosition;
            Vector2 pos = new Vector2();
            float t = 0;

            while (pos != toPos || t < 1)
            {
                pos = Vector2.Lerp(from, toPos, ease.Evaluate(t));
                t += Time.deltaTime / tweenTime;
                SetContentAnchoredPosition(pos);

                yield return null;
            }

            callback?.Invoke();
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
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position,
                eventData.pressEventCamera, out localCursor))
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
            {
                position.x = content.anchoredPosition.x;
            }
            else if (swipeType == SwipeType.Horizontal)
            {
                position.y = content.anchoredPosition.y;
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

            min.x += pointerDelta.x;
            max.x += pointerDelta.x;
            if (min.x > viewBounds.min.x)
                offset.x = viewBounds.min.x - min.x;
            else if (max.x < viewBounds.max.x)
                offset.x = viewBounds.max.x - max.x;

            return offset;
        }

        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize *
                   Mathf.Sign(overStretching);
        }

        #endregion


        #region Editor

        private void Reset()
        {
            maskContent = true;
            content = transform.GetChild(0) as RectTransform;
            swipeTime = 0.5f;
            swipeVelocityThreshold = 50;
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