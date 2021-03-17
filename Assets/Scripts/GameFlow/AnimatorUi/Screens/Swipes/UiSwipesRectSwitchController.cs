using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using DG.Tweening;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSwipesRectSwitchController : IDeinitializable
    {
        #region Fields

        private const float NotChoosedElementsScaleMultiplier = 0.8f;
        private const float shownDeltaWidth = 70.0f;

        [SerializeField] private ScreenSwipe screenSwipe = default;
        [SerializeField] private VectorAnimation scaleAnimation = default;

        private List<IUiSwipesRect> rectTransforms;

        private Action OnTabChanged;

        private Coroutine expandCoroutine;
        private IUiSwipesRect previous;

        #endregion



        #region Properties

        public IUiSwipesRect Current
        {
            get
            {
                bool isInRange = screenSwipe.CurrentScreen >= 0 && screenSwipe.CurrentScreen < rectTransforms.Count;
                return isInRange ? rectTransforms[screenSwipe.CurrentScreen] : default;
            }
        }

        #endregion



        #region Methods

        public void Initialize(Action _onTabChanged)
        {
            rectTransforms = new List<IUiSwipesRect>();
            OnTabChanged = _onTabChanged;
            OnTabChanged += () => RefreshSwitchButtonsState();

            screenSwipe.shouldSkipInactiveScreens = true;
            screenSwipe.onScreenChanged += OnScreenChanged;
        }


        public void Deinitialize()
        {
            screenSwipe.onScreenChanged -= OnScreenChanged;

            DOTween.Kill(this);
            MonoBehaviourLifecycle.StopPlayingCorotine(expandCoroutine);
            expandCoroutine = null;

            OnTabChanged = null;
        }


        public void AddScreen(IUiSwipesRect target)
        {
            rectTransforms.Add(target);
            screenSwipe.AddScreen(target.SwipeRect, int.MaxValue);
        }


        public void RemoveScreen(IUiSwipesRect target)
        {
            int swipeScreenIndex = screenSwipe.IndexOf(target.SwipeRect);

            if (swipeScreenIndex == screenSwipe.CurrentScreen &&
                screenSwipe.ScreenCount > 0)
            {
                screenSwipe.GoToScreen(0);
            }

            rectTransforms.Remove(target);
            screenSwipe.RemoveScreen(swipeScreenIndex, false);
        }   


        public void AddOnTabChangedCallback(Action callback) =>
            OnTabChanged += callback;


        public void RemoveOnTabChangedCallback(Action callback) =>
            OnTabChanged -= callback;


        public void ToNext() =>
            screenSwipe.GoToNextScreen();


        public void ToPrev() =>
            screenSwipe.GoToPreviousScreen();



        public void SwitchTo(int index, bool isImmediately = false)
        {
            screenSwipe.GoToScreen(index, isImmediately);

            // hack crunch logic cuz of bad callbacks lifecycle in screenSwipe.cs
            OnScreenChanged(index);
        }


        public void SwitchTo(IUiSwipesRect target, bool isImmediately = false)
        {
            IUiSwipesRect foundTarget = rectTransforms.FirstOrDefault(e => e == target);

            if (foundTarget == null)
            {
                CustomDebug.Log($"Attempt to switch to null target in {this}");
                return;
            }

            int currentIndex = screenSwipe.IndexOf(target.SwipeRect);
            SwitchTo(currentIndex, isImmediately);
        }


        public void PlaySetupSpacingContentCoroutine(float canvasScaleFactor, float elementsWidth)
        {
            expandCoroutine = MonoBehaviourLifecycle.PlayCoroutine(SetupElemenetsSpacing(canvasScaleFactor));

            IEnumerator SetupElemenetsSpacing(float _canvasScaleFactor)
            {
                yield return new WaitForEndOfFrame();

                float space = ((Screen.width / _canvasScaleFactor - elementsWidth) * 0.5f) + 2.0f * shownDeltaWidth;

                screenSwipe.Spacing = -space;
            }
        }


        private void RefreshSwitchButtonsState()
        {
            if (rectTransforms.IsNullOrEmpty())
            {
                return;
            }

            bool isIndexInRange = screenSwipe.CurrentScreen >= 0 && screenSwipe.CurrentScreen < rectTransforms.Count;

            if (!isIndexInRange)
            {
                CustomDebug.Log($"Index {screenSwipe.CurrentScreen} is not in range int {this}");
                return;
            }

            IUiSwipesRect current = rectTransforms.ElementAt(screenSwipe.CurrentScreen);

            foreach (var rectTransform in rectTransforms)
            {
                bool isCurrentRectSelected = rectTransform == current;
                bool shouldAnimateScale = isCurrentRectSelected || rectTransform == previous;

                if (shouldAnimateScale)
                {

                    Vector3 begin = isCurrentRectSelected ? rectTransform.ScalableSwipeRect.localScale : Vector3.one;
                    Vector3 end = isCurrentRectSelected ? Vector3.one : (Vector3.one * NotChoosedElementsScaleMultiplier);

                    scaleAnimation.SetupBeginValue(begin);
                    scaleAnimation.SetupEndValue(end);

                    scaleAnimation.Play((value) => rectTransform.ScalableSwipeRect.localScale = value, this);
                }
                else
                {
                    rectTransform.ScalableSwipeRect.localScale = (isCurrentRectSelected) ? Vector3.one : Vector3.one * NotChoosedElementsScaleMultiplier;
                }

                rectTransform.SetButtonsEnabled(isCurrentRectSelected);
            }

            previous = current;
        }


        private void OnScreenChanged(int value) =>
            OnTabChanged?.Invoke();
        
        #endregion 
    }
}
