using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;


namespace Drawmasters.Ui
{
    public class UiProgressBar : MonoBehaviour
    {
        #region Fields

        [SerializeField] protected Image fillAreaImage = default;

        [SerializeField] protected FactorAnimation progressBarFillAnimation = default;

        private float bottomBound;
        private float topBound;

        protected float currentValue;

        #endregion



        #region Class Lifecycle

        public virtual void Initialize() { }


        public virtual void Deinitialize() =>
            DOTween.Kill(this);

        #endregion



        #region Methods

        public void UpdateProgress(float value) =>
            UpdateProgress(currentValue, value);

        public void UpdateProgress(float from, float to)
        {
            currentValue = to;
            fillAreaImage.fillAmount = CalculateNormalizedProgress(from); // HACK?

            if (Mathf.Approximately(from, to)) // TODO: Not explicit. Replace with bool isImmediatelly. To Dmitry s 
            {
                return;
            }

            OnUpdateProgress(from, to);
        }


        public void SetBounds(float bottom, float top)
        {
            bottomBound = bottom;
            topBound = top;
        }


        protected virtual void OnUpdateProgress(float from, float to, Action<float> onValueChanged = default)
        {
            currentValue = to;
            float targetProgress = CalculateNormalizedProgress(to);

            progressBarFillAnimation.beginValue = fillAreaImage.fillAmount;
            progressBarFillAnimation.endValue = targetProgress;

            progressBarFillAnimation.Play((value) =>
            {
                fillAreaImage.fillAmount = value;
                onValueChanged?.Invoke(value);
            }, this);
        }


        protected float CalculateNormalizedProgress(float value)
        {
            float normalizedValue = Mathf.Clamp(value, bottomBound, topBound) - bottomBound;

            float progress = normalizedValue / topBound;

            return progress;
        }

        #endregion
    }
}
