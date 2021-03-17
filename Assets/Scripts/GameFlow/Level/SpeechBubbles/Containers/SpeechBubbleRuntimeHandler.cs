using System;
using UnityEngine;
using DG.Tweening;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public abstract class SpeechBubbleRuntimeHandler : MonoBehaviour, IInitializable, IDeinitializable
    {
        #region Helpers

        private static class Keys
        {
            public static int StartAnimationHash = Animator.StringToHash("LoserBubbleShow");

            public static int ShowTriggerHash = Animator.StringToHash("Show");

            public static int HideTriggerHash = Animator.StringToHash("Hide");
        }

        #endregion



        #region Fields

        [SerializeField] private Animator mainAnimator = default;

        private HideState hideStateMachine;

        private Action delayedCallback;

        #endregion


        #region IInitializable

        public virtual void Initialize()
        {
            Hide(null, true);
        }

        #endregion



        #region IDeinitializable

        public virtual void Deinitialize()
        {
            if (hideStateMachine != null)
            {
                hideStateMachine.OnHideEnd -= HideStateMachine_OnHideEnd;
            }

            DOTween.Kill(this, true);
        }

        #endregion



        #region Methods

        public virtual void Show()
        {
            CommonUtility.SetObjectActive(gameObject, true);

            hideStateMachine = mainAnimator.GetBehaviour<HideState>();
            hideStateMachine.Initialize(mainAnimator);
            hideStateMachine.OnHideEnd += HideStateMachine_OnHideEnd;

            mainAnimator.Play(Keys.StartAnimationHash);
            mainAnimator.SetTrigger(Keys.ShowTriggerHash);
        }


        public virtual void Hide(Action onHided = null, bool isImmediately = false)
        {
            if (isImmediately)
            {
                OnHided(onHided);
            }
            else
            {
                delayedCallback = onHided;

                mainAnimator.SetTrigger(Keys.HideTriggerHash);
            }
        }

        #endregion



        #region Events handlers

        private void HideStateMachine_OnHideEnd()
        {
            mainAnimator.ResetTrigger(Keys.ShowTriggerHash);
            mainAnimator.ResetTrigger(Keys.HideTriggerHash);

            OnHided(delayedCallback);

            delayedCallback = null;
        }


        private void OnHided(Action onHided)
        {
            if (hideStateMachine != null)
            {
                hideStateMachine.OnHideEnd -= HideStateMachine_OnHideEnd;
                hideStateMachine = null;
            }

            CommonUtility.SetObjectActive(gameObject, false);

            onHided?.Invoke();
        }

        #endregion
    }
}
