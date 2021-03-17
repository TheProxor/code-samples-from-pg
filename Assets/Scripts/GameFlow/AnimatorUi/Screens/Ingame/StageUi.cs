using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace Drawmasters.Ui
{
    public class StageUi : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image completedImage = default;

        [SerializeField] private FactorAnimation levelWinScaleAnimation = default;
        [SerializeField] private FactorAnimation levelWinAlphaAnimation = default;

        #endregion



        #region Methods

        public void Initialize(bool isCompleted)
        {
            float alpha = isCompleted ? 1.0f : 0.0f;
            completedImage.color = completedImage.color.SetA(alpha);
        }


        public void Deinitialize() =>
            DOTween.Kill(this, true);


        public void PlayCompleteAnimation()
        {
            levelWinScaleAnimation.Play(value => 
                transform.localScale = Vector3.one * value, this);
            
            levelWinAlphaAnimation.Play(value => 
                completedImage.color = completedImage.color.SetA(value), this);
        }

        #endregion
    }
}
