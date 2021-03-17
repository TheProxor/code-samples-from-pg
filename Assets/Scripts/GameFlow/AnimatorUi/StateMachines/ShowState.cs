using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Ui
{ 
    public class ShowState : StateMachineBehaviour
    {
        #region Fields

        public event Action OnShowBegin;
        public event Action OnShowEnd;

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
                OnShowBegin?.Invoke();
            }
        }


        public override void OnStateExit(Animator _animator, 
                                         AnimatorStateInfo stateInfo, 
                                         int layerIndex)
        {
            if (animator == _animator)
            {
                OnShowEnd?.Invoke();
            }
        }

        #endregion
    }
}
