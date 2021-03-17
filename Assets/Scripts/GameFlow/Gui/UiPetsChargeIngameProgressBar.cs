using System;
using Drawmasters.Pets;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiPetsChargeIngameProgressBar : UiProgressBar
    {
        #region Fields

        [SerializeField] private RectTransform[] barEndRectTransforms = default;
        [SerializeField] float barEndImageRectMultiplier = 1.0f; // for corrent initial values

        private PetSkinType petSkinType;
        private PetsChargeController petsChargeController;

        #endregion


        
        #region Class Lifecycle

        public override void Initialize()
        {
            base.Initialize();

            petsChargeController = GameServices.Instance.PetsService.ChargeController;

            SetBounds(petsChargeController.MinChargePoints, petsChargeController.MaxChargePoints);
            float petsChargePoints = petsChargeController.CurrentChargePointsCount;
            UpdateProgress(petsChargePoints, petsChargePoints);
            
            RefreshBarEndImage();
        }
        
        
        protected override void OnUpdateProgress(float from, float to, Action<float> onValueChanged = null)
        {
            if (this is null)
            {
                CustomDebug.Log($"There must be NullReferenceException. Preventing exception in {this}.");
                Deinitialize();
            }

            onValueChanged += (value) =>
            {
                RefreshBarEndImage();
            };

            base.OnUpdateProgress(from, to, onValueChanged);

            RefreshBarEndImage();
        }


        private void RefreshBarEndImage()
        {
            if (fillAreaImage == null ||
                fillAreaImage.rectTransform == null)
            {
                CustomDebug.Log($"There must be NullReferenceException. Preventing exception in {this}.");
                Deinitialize();
                return;
            }

            float angle = Mathf.Lerp(0.0f, 360.0f, fillAreaImage.fillAmount) + 90.0f;
            Vector3 imageDirection = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;

            Rect rectToCheck = fillAreaImage.rectTransform.rect.ScaleSizeBy(barEndImageRectMultiplier);
            bool isIntersects = CommonUtility.LineRectIntersectionByDirection(fillAreaImage.rectTransform.anchoredPosition, imageDirection, rectToCheck, out Vector3 intersectionPoint);

            bool shouldShowBarEndRectTransforms = !Mathf.Approximately(0.0f, fillAreaImage.fillAmount) &&
                                                  !Mathf.Approximately(1.0f, fillAreaImage.fillAmount) &&
                                                  isIntersects;
            foreach (var barEndRectTransform in barEndRectTransforms)
            {
                if (isIntersects)
                {
                    barEndRectTransform.anchoredPosition = intersectionPoint;
                }

                CommonUtility.SetObjectActive(barEndRectTransform.gameObject, shouldShowBarEndRectTransforms);
            }
        }
        
        #endregion

    }
}
