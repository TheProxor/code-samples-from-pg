using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class AlternativeKeyButton : Button
    {
        #region Fields

        [SerializeField] private List<KeyCode> buttonPressKeys = default;

        private bool isKeysPressed = false;
        
        #endregion



        #region Properties

        private bool IsKeysPressed
        {
            get => isKeysPressed;
            set
            {
                if (isKeysPressed != value)
                {
                    isKeysPressed = value;

                    if (isKeysPressed)
                    {
                        onClick.Invoke();
                    }
                }
            }
        }

        #endregion



        #region Unity lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            bool isButtonPressed = true;
            foreach (KeyCode buttonPressKey in buttonPressKeys)
            {
                isButtonPressed &= Input.GetKey(buttonPressKey);
            }

            IsKeysPressed = isButtonPressed;
        }

        #endregion
    }
}
