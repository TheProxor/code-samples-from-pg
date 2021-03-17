using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Modules.General;


namespace Drawmasters.Utils.Ui
{
    public class ShowAndMoveElementsHelper
    {
        #region Fields

        private readonly VectorAnimation animation;
        private readonly FactorAnimation factorAnimation;
        
        #endregion



        #region Class lifecycle

        public ShowAndMoveElementsHelper(VectorAnimation _animation, FactorAnimation _factorAnimation)
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
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void MoveLayoutElement(Vector2 startPosition, Vector2 finishPosition, 
            LayoutElement targetLayoutElement, CanvasGroup canvasGroup, Action callback = default)
        {
            DOTween.Complete(this, true);

            int targetLayoutElementSiblingIndex = targetLayoutElement.transform.GetSiblingIndex();

            targetLayoutElement.ignoreLayout = true;
            ((RectTransform) targetLayoutElement.transform).anchoredPosition = startPosition;
            
            Scheduler.Instance.CallMethodWithDelay(this, PlayAnimations, 0.0f);


            void PlayAnimations()
            {
                factorAnimation.Play((value) =>
                {
                    canvasGroup.alpha = value;
                }, this);

                animation.SetupBeginValue(startPosition);
                animation.SetupEndValue(finishPosition);
                 
                animation.Play((value) =>
                {
                    ((RectTransform) targetLayoutElement.transform).anchoredPosition = value;
                }, this, () =>
                {
                    targetLayoutElement.transform.SetSiblingIndex(targetLayoutElementSiblingIndex);
                    targetLayoutElement.ignoreLayout = false;
                    
                    callback?.Invoke();
                });
            }
        }
        
        #endregion
    }
}
