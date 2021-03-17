using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Modules.General;
using Object = UnityEngine.Object;


namespace Drawmasters.Utils.Ui
{
    public class ScrollElementsMoveUtility
    {
        #region Fields

        private readonly VectorAnimation animation;
        private readonly FactorAnimation factorAnimation;

        private readonly List<LayoutElement> placeHoldersElements = new List<LayoutElement>();

        #endregion



        #region Class lifecycle

        public ScrollElementsMoveUtility(VectorAnimation _animation, FactorAnimation _factorAnimation)
        {
            animation = _animation;
            factorAnimation = _factorAnimation;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
        }


        public void Deinitialize()
        {
            foreach (var element in placeHoldersElements)
            {
                DestroyLayoutElement(element);
            }

            placeHoldersElements.Clear();

            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void MoveLayoutElement(RectTransform content, LayoutElement targetLayoutElement, int targetSiblingIndex, Action<Vector3> onMoving = default, Action callback = default)
        {
            DOTween.Complete(this, true);

            int targetLayoutElementSiblingIndex = targetLayoutElement.transform.GetSiblingIndex();

            if (targetLayoutElementSiblingIndex == targetSiblingIndex)
            {
                callback?.Invoke();
                return;
            }

            bool isDownMove = targetSiblingIndex < targetLayoutElementSiblingIndex;
            int siblingIndexToSet = isDownMove ? targetSiblingIndex : targetSiblingIndex + 2;

            float targetLayoutElementHeight = targetLayoutElement.preferredHeight;
            LayoutElement currentPlaceElement = CreatePlaceHolderLayoutElement(content, targetLayoutElementHeight);
            RectTransform currentPlaceElementRectTransform = currentPlaceElement.transform as RectTransform;
            currentPlaceElement.transform.SetSiblingIndex(targetLayoutElementSiblingIndex);

            LayoutElement targetPlaceElement = CreatePlaceHolderLayoutElement(content, 0.0f);
            RectTransform targetPlaceElementRectTransform = targetPlaceElement.transform as RectTransform;
            targetPlaceElement.transform.SetSiblingIndex(siblingIndexToSet);

            RectTransform targetLayoutElementRectTransform = targetLayoutElement.transform as RectTransform;
            targetLayoutElementRectTransform.anchorMax = Vector2.zero.SetY(1.0f);
            targetLayoutElementRectTransform.anchorMin = Vector2.zero.SetY(1.0f);
            
            targetLayoutElement.ignoreLayout = true;
            targetLayoutElement.transform.SetAsLastSibling();

            placeHoldersElements.Add(currentPlaceElement);
            placeHoldersElements.Add(targetPlaceElement);

            Scheduler.Instance.CallMethodWithDelay(this, PlayAnimations, 0.0f);


            void PlayAnimations()
            {
                factorAnimation.Play((value) =>
                {
                    float currentPlaceElementHeight = targetLayoutElementHeight * (1 - value);
                    currentPlaceElement.preferredHeight = currentPlaceElementHeight;
                    currentPlaceElementRectTransform.sizeDelta = currentPlaceElementRectTransform.sizeDelta.SetY(currentPlaceElementHeight);

                    float targetPlaceElementHeight = targetLayoutElementHeight * value;
                    targetPlaceElement.preferredHeight = targetPlaceElementHeight;
                    targetPlaceElementRectTransform.sizeDelta = targetPlaceElementRectTransform.sizeDelta.SetY(targetPlaceElementHeight);

                }, this);

                animation.SetupBeginValue(currentPlaceElementRectTransform.anchoredPosition);

                // TODO: Add logic if vertical layout has spacing
                float elementOffset = isDownMove ? -targetLayoutElementHeight * 0.5f : targetLayoutElementHeight * 0.5f;
                float animationEndValueY = targetPlaceElementRectTransform.anchoredPosition.y + elementOffset;

                animation.SetupEndValue(targetPlaceElementRectTransform.anchoredPosition.SetY(animationEndValueY));
                 
                animation.Play((value) =>
                {
                    targetLayoutElementRectTransform.anchoredPosition = value;

                    onMoving?.Invoke(value);
                }, this, () =>
                {
                    targetLayoutElement.transform.SetSiblingIndex(targetSiblingIndex);
                    targetLayoutElement.ignoreLayout = false;

                    placeHoldersElements.Remove(currentPlaceElement);
                    DestroyLayoutElement(currentPlaceElement);

                    placeHoldersElements.Remove(targetPlaceElement);
                    DestroyLayoutElement(targetPlaceElement);

                    callback?.Invoke();
                });
            }
        }


        private LayoutElement CreatePlaceHolderLayoutElement(Transform root, float preferredHeight)
        {
            GameObject placeObject = new GameObject("PlaceHolderLayoutElement", typeof(RectTransform));
            placeObject.transform.SetParent(root);
            placeObject.transform.localScale = Vector3.one;

            LayoutElement placeElement = placeObject.AddComponent<LayoutElement>();
            placeElement.preferredHeight = preferredHeight;

            RectTransform placeElementRectTransform = placeObject.transform as RectTransform;
            placeElementRectTransform.anchoredPosition3D = Vector3.one;
            placeElementRectTransform.sizeDelta = placeElementRectTransform.sizeDelta.SetY(preferredHeight);

            return placeElement;
        }


        private void DestroyLayoutElement(LayoutElement layoutElement)
        {
            Object.Destroy(layoutElement.gameObject);
        }

        #endregion
    }
}
