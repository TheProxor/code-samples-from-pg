using UnityEngine;
using DG.Tweening;
using System;


namespace Drawmasters.Announcer
{
    public class CommonAnnouncer : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            public VectorAnimation moveAnimation = default;
            public FactorAnimation alphaAnimation = default;
            public VectorAnimation scaleAnimation = default;

        }

        #endregion



        #region Fields

        [SerializeField] private CanvasGroup canvasGroup = default;

        private Data data;

        #endregion



        #region Properties

        public bool IsTweenActive =>
            DOTween.IsTweening(this);

        #endregion



        #region Methods

        public void SetupData(Data _data) =>
            data = _data;


        public void SetupData(VectorAnimation _moveAnimation,
                              FactorAnimation _alphaAnimation = default,
                              VectorAnimation _scaleAnimation = default)
        {
            data = new Data
            {
                moveAnimation = _moveAnimation,
                alphaAnimation = _alphaAnimation,
                scaleAnimation = _scaleAnimation
            };
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
        }


        public void PlayLocal(Vector3 beginPosition, Vector3 offset)
        {
            DOTween.Complete(this);

            transform.localPosition = beginPosition;

            data.moveAnimation.beginValue = beginPosition;
            data.moveAnimation.endValue = beginPosition + offset;

            data.moveAnimation.Play((value) => transform.localPosition = value, this);

            float startAlphaValue = data.alphaAnimation == null ? 1.0f : 0.0f;
            canvasGroup.alpha = startAlphaValue;

            data.alphaAnimation?.Play((value) => canvasGroup.alpha = value, this);
            data.scaleAnimation?.Play((value) => transform.localScale = value, this);
        }

        #endregion
    }
}
