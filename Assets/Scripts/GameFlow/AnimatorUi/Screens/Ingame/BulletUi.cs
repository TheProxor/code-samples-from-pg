using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;


namespace Drawmasters.Levels
{
    [Serializable]
    public class BulletUi
    {
        #region Fields

        [SerializeField] private Image enabled = default;
        [SerializeField] private Image disabled = default;
        [SerializeField] public GameObject parentGameObject = default;

        private VectorAnimation scaleOutAnimation;
        private VectorAnimation scaleInAnimation;

        #endregion



        #region Properties

        public Vector3 Position => parentGameObject.transform.position;

        #endregion



        #region Methods

        public void Initialize()
        {
            CommonUtility.SetObjectActive(parentGameObject, true);

            ChangeState(true);
        }


        public void Deinitialize() => DOTween.Kill(this);
        

        public void InitializeVisual(IngameBulletsUi.Data data)
        {
            if (data != null)
            {
                enabled.sprite = data.enabledSprite;
                enabled.SetNativeSize();

                disabled.sprite = data.disabledSprite;
                disabled.SetNativeSize();

                disabled.color = data.disabledColor;
            }
        }


        public void InitializeAnimation(VectorAnimation _scaleInAnimation,
                                        VectorAnimation _scaleOutAnimation)
        {
            scaleOutAnimation = _scaleOutAnimation;
            scaleInAnimation = _scaleInAnimation;
        }


        public void Disable() => CommonUtility.SetObjectActive(parentGameObject, false);
        

        public void CompleteAnimation() => DOTween.Complete(this);


        public void ChangeState(bool isEnabled)
        {
            CommonUtility.SetObjectActive(disabled.gameObject, !isEnabled);

            CompleteAnimation();

            if (isEnabled)
            {
                enabled.transform.localScale = Vector3.zero;
                scaleInAnimation.Play(value => enabled.transform.localScale = value, this);
            }
            else
            {
                scaleOutAnimation.Play(value => enabled.transform.localScale = value, this);
            }
        }

        #endregion
    }
}
