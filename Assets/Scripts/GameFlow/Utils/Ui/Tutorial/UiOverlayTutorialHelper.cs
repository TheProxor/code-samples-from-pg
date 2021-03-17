using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;


namespace Drawmasters.Ui
{
    public class UiOverlayTutorialHelper
    {
        #region Fields

        private int proposeSortingOrder = 16;

        private readonly Image fadeImage;
        private readonly FactorAnimation fadeAnimation;
        private readonly bool shouldAddGraphicRaycast;

        private readonly List<Canvas> tutorialCanvases;
        private readonly List<GraphicRaycaster> tutorialGraphicRaycasters;

        private IUiOverlayTutorialObject[] overlayElements;

        #endregion



        #region Class lifecycle

        public UiOverlayTutorialHelper(Image _fadeImage, FactorAnimation _fadeAnimation, bool _shouldAddGraphicRaycast = false)
        {
            fadeImage = _fadeImage;
            fadeAnimation = _fadeAnimation;
            shouldAddGraphicRaycast = _shouldAddGraphicRaycast;

            tutorialCanvases = new List<Canvas>();
            tutorialGraphicRaycasters = new List<GraphicRaycaster>();
        }

        #endregion



        #region Methods


        public void SetupOverlayedObjects(params IUiOverlayTutorialObject[] _overlayElements)
        {
            overlayElements = _overlayElements;
        }


        public void SetupSortingOrder(int sortingOrder) =>
            proposeSortingOrder = sortingOrder;


        public void Initialize()
        {
        }


        public void Deinitialize()
        {
            DestroyTutorialCanvases();
            SetFadeEnabled(false);

            DOTween.Kill(this);
        }


        public void PlayTutorial()
        {
            SetFadeEnabled(true);
            fadeImage.color = fadeImage.color.SetA(default);
            fadeAnimation.Play(value => fadeImage.color = fadeImage.color.SetA(value), this);

            foreach (var element in overlayElements)
            {
                Canvas tutorialCanvas = element.OverlayTutorialObject.AddComponent<Canvas>();
                tutorialCanvas.overrideSorting = true;
                tutorialCanvas.sortingLayerName = "UI";

                tutorialCanvas.sortingOrder = proposeSortingOrder;

                tutorialCanvases.Add(tutorialCanvas);

                if (shouldAddGraphicRaycast)
                {
                    GraphicRaycaster tutorialGR = element.OverlayTutorialObject.AddComponent<GraphicRaycaster>();
                    tutorialGraphicRaycasters.Add(tutorialGR);
                }
            }
        }


        public void StopTutorial(Action callback)
        {
            fadeAnimation.Play(value => fadeImage.color = fadeImage.color.SetA(value), this, () =>
            {
                DestroyTutorialCanvases();
                SetFadeEnabled(false);

                callback?.Invoke();
            }, isReversed: true);
        }


        public void SetInputEnabled(bool value)
        {
            foreach (var raycaster in tutorialGraphicRaycasters)
            {
                raycaster.enabled = value;
            }
        }


        private void SetFadeEnabled(bool enabled) =>
            CommonUtility.SetObjectActive(fadeImage.gameObject, enabled);


        private void DestroyTutorialCanvases()
        {
            foreach (var tutorialGraphicRaycaster in tutorialGraphicRaycasters)
            {
                Object.Destroy(tutorialGraphicRaycaster);
            }
            tutorialGraphicRaycasters.Clear();

            foreach (var tutorialCanvas in tutorialCanvases)
            {
                Object.Destroy(tutorialCanvas);
            }
            tutorialCanvases.Clear();
        }

        #endregion
    }
}
