using Modules.General;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class TutorialButton : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Animator buttonAnimator = default;
        [SerializeField] private Button okButton = default;

        #endregion



        #region Methods

        public void Initialize(float enableDelay)
        {
            okButton.enabled = false;
            buttonAnimator.Play(AnimationKeys.TutorialOkButton.Start);

            Scheduler.Instance.CallMethodWithDelay(this, Show, enableDelay);
        }


        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        private void Show()
        {
            buttonAnimator.SetTrigger(AnimationKeys.TutorialOkButton.Show);
            okButton.enabled = true;
        }

        #endregion
    }
}

