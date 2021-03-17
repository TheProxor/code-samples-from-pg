using UnityEngine;
using Drawmasters.Pets;
using Drawmasters.ServiceUtil;
using System;


namespace Drawmasters.Ui
{
    public class UiPetsChargeProgressBar : UiProgressBar
    {
        #region Fields

        [SerializeField] private RectTransform[] barEndRectTransforms = default;
        [SerializeField] float barEndImageRectMultiplier = 1.0f; // for corrent initial values

        private PetSkinType petSkinType;
        private PetsChargeController petsChargeController;

        #endregion



        #region Methods

        public void SetupPetSkinType(PetSkinType _petSkinType) =>
            petSkinType = _petSkinType;

        #endregion



        #region Class Lifecycle

        public override void Initialize()
        {
            base.Initialize();

            petsChargeController = GameServices.Instance.PetsService.ChargeController;
            petsChargeController.OnChargePointsApplied += ChargeController_OnChargePointsApplied;

            SetBounds(petsChargeController.MinChargePoints, petsChargeController.MaxChargePoints);
            float petsChargePoints = petsChargeController.CurrentChargePointsCount;
            UpdateProgress(petsChargePoints, petsChargePoints);

            RefreshBarEndImage();
        }


        public override void Deinitialize()
        {
            if (petsChargeController != null)
            {
                petsChargeController.OnChargePointsApplied -= ChargeController_OnChargePointsApplied;
            }

            base.Deinitialize();
        }


        protected override void OnUpdateProgress(float from, float to, Action<float> onValueChanged = null)
        {
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
                Debug.Break();
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



        #region Event Handlers

        private void ChargeController_OnChargePointsApplied(PetSkinType chargedPetSkinType, float recievedPoints)
        {
            if (petSkinType != chargedPetSkinType)
            {
                return;
            }

            float petsChargePoints = petsChargeController.CurrentChargePointsCount;
            UpdateProgress(currentValue, petsChargePoints);
        }

        #endregion
    }
}
