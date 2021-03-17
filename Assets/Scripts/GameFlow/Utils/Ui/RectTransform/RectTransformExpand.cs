using System;
using Modules.General;
using TMPro;
using UnityEngine;

namespace Drawmasters.Ui
{
    public class RectTransformExpand : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Fields

        [SerializeField] private RectTransform currentRectTransform = default;
        [SerializeField] private TMP_Text textToCheck = default;
        [SerializeField] private Vector2 expandFactor = Vector2.one;

        #endregion


        #region Methods

        public void Initialize()
        {
            if (currentRectTransform == null)
            {
                currentRectTransform = GetComponent<RectTransform>();
            }

            textToCheck.OnPreRenderText += OnPreRenderText;

            Refresh();
        }

        public void Deinitialize()
        {
            textToCheck.OnPreRenderText -= OnPreRenderText;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void Refresh()
        {
            Vector2 maxAnotherSize = Vector2.Min(textToCheck.GetPreferredValues(), textToCheck.rectTransform.sizeDelta);
            Vector3 targetSize = maxAnotherSize * expandFactor;
            currentRectTransform.sizeDelta = targetSize.ToVector2();
        }

        #endregion


        #region Events handlers

        private void OnPreRenderText(TMP_TextInfo textInfo)
        {
            // To avoid changing graphic while canvas is being rendered
            Scheduler.Instance.CallMethodWithDelay(this, Refresh, CommonUtility.OneFrameDelay);
        }

        #endregion
    }
}
