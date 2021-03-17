using System;
using UnityEngine;



namespace Drawmasters.Ui
{
    public class HideState : StateMachineBehaviour
    {
        #region Fields

        public event Action OnHideBegin;
        public event Action OnHideEnd;

        private Animator animator;

        #endregion



        #region Methods

        public void Initialize(Animator _animator)
        {
            animator = _animator;
        }

        #endregion



        #region Messages

        public override void OnStateEnter(Animator _animator,
                                          AnimatorStateInfo stateInfo,
                                          int layerIndex)
        {
            if (animator == _animator)
            {
                OnHideBegin?.Invoke();
            }
        }


        public override void OnStateExit(Animator _animator,
                                         AnimatorStateInfo stateInfo,
                                         int layerIndex)
        {
            if (animator == _animator)
            {
                OnHideEnd?.Invoke();
            }
        }

        #endregion
    }
}
