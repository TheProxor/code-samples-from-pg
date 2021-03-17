using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorStateChangeReactor : MonoBehaviour
    {
        #region Variables

        [SerializeField] private List<LevelEditor.State> acitvityStates = default;
        [SerializeField] private List<MonoBehaviour> componentsToDisable = default;
        [SerializeField] private bool shouldChangeGameObjectActivity = default;

        #endregion



        #region Unity Lifecycle

        private void Awake() => ChangeState.Subscribe(OnStateChange);

        
        private void OnDestroy() => ChangeState.Unsubscribe(OnStateChange);


        private void Start() =>  OnStateChange(LevelEditor.CurrentState);

        #endregion



        #region Events Handlers

        private void OnStateChange(LevelEditor.State newState)
        {
            bool isActiveState = acitvityStates.Contains(newState);

            foreach (var component in componentsToDisable)
            {
                component.enabled = isActiveState;
            }

            if (shouldChangeGameObjectActivity)
            {
                if (gameObject != null)
                {
                    gameObject.SetActive(isActiveState);
                }
            }
        }

        #endregion
    }
}
