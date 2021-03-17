using System;
using Drawmasters.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters.Ui
{
    public class TutorialFingerScreen : AnimatorScreen, ITutorialScreen, IBeginDragHandler, IDragHandler, IPointerDownHandler
    {
        #region Fields

        [SerializeField] private int sortingOffset = default;

        [SerializeField] private Transform fingerParent = default;
        [SerializeField] private RectTransform inputCheckTransform = default;

        private Action completeTutorialCallback;

        private Animator fingerAnimator;

        private bool shouldCloseOnDown;
        private int savedAdditionalOrderOffset;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.TutorialFinger;

        #endregion



        #region Methods

        public void Initialize(TutorialType type, Action _completeTutorialCallback)
        {
            TutorialSettings.FingersTutorialData data = IngameData.Settings.tutorialSettings.FindFingerTutorialData(type);

            if (data == null)
            {
                HideImmediately();
            }
            else
            {
                fingerAnimator = Instantiate(data.fingerAnimator, fingerParent);
                SetFingerAnimatorTrigger(AnimationKeys.TutorialFinger.Show);
            }

            completeTutorialCallback = _completeTutorialCallback;
        }


        public override void Deinitialize()
        {
            if (fingerAnimator != null)
            {
                Content.Management.DestroyObject(fingerAnimator.gameObject);
                fingerAnimator = null;
            }

            mainCanvas.sortingOrder -= sortingOffset;
            mainCanvas.sortingOrder -= savedAdditionalOrderOffset;

            base.Deinitialize();
        }


        public override void Show()
        {
            base.Show();

            //SetFingerAnimatorTrigger(AnimationKeys.TutorialFinger.Show);
            mainCanvas.sortingOrder += sortingOffset;
        }

        public override void Hide()
        {
            SetFingerAnimatorTrigger(AnimationKeys.TutorialFinger.Hide);

            base.Hide();
        }


        public override void InitializeButtons() { }


        public override void DeinitializeButtons() { }

        public void SetSortingOrder(int order)
        {
            savedAdditionalOrderOffset = order - mainCanvas.sortingOrder;

            mainCanvas.sortingOrder += savedAdditionalOrderOffset;

        }

        public void MarkShouldCloseOnDown() =>
            shouldCloseOnDown = true;


        public void RepositionInputCheckZone(Vector3 position) =>
            inputCheckTransform.anchoredPosition3D = position;


        public void ResizeInputCheckZone(Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            inputCheckTransform.anchorMin = anchorMin;
            inputCheckTransform.anchorMax = anchorMax;
            inputCheckTransform.sizeDelta = size;
        }

        private void SetFingerAnimatorTrigger(string key)
        {
            if (fingerAnimator != null)
            {
                fingerAnimator.SetTrigger(key);
            }
        }


        private void EndTutorial()
        {
            completeTutorialCallback?.Invoke();

            HideImmediately();
        }

        #endregion



        #region IDragHandler

        public void OnDrag(PointerEventData eventData) { }

        #endregion



        #region IBeginDragHandler

        public void OnBeginDrag(PointerEventData eventData) =>
            EndTutorial();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (shouldCloseOnDown)
            {
                EndTutorial();
            }
        }

        #endregion
    }
}
