using System;
using Drawmasters.Tutorial;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiPetsChargeTutorialScreen : AnimatorScreen, ITutorialScreen
    {
        #region Nested Types

        [Serializable]
        public class BehaviourData
        {
            public TutorialType tutorialType = default;
            public GameObject root = default;
            public Animator petVisualPrefab = default;

            public Image fadeImage = default;
            public FactorAnimation fadeAnimation = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Button closeButton = default;
        [Header("Behaviours data")]
        [SerializeField] private UiPetsChargeTutorialBehaviour.Data chargeData = default;
        [SerializeField] private UiPetsInvokeTutorialBehaviour.Data invokeData = default;
        
        private Action completeTutorialCallback;

        private IUiBehaviour currentBehaviour;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
            ScreenType.PetsChargeTutorial;

        #endregion



        #region Methods

        public void Initialize(TutorialType type, Action _completeTutorialCallback)
        {
            completeTutorialCallback = _completeTutorialCallback;

            if (type == TutorialType.PetChargePoints)
            {
                currentBehaviour = new UiPetsChargeTutorialBehaviour(this, chargeData);
            }
            else if (type == TutorialType.PetInvoke)
            {
                currentBehaviour = new UiPetsInvokeTutorialBehaviour(this, invokeData);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic in {this}");
                CompleteTutorial();
                return;
            }

            currentBehaviour.Enable();

            currentBehaviour.DeinitializeButtons();
            currentBehaviour.InitializeButtons();
        }


        public override void Deinitialize()
        {
            currentBehaviour?.Disable();
            currentBehaviour?.Deinitialize();

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            closeButton.onClick.AddListener(CompleteTutorial);

            currentBehaviour?.InitializeButtons();
        }


        public override void DeinitializeButtons()
        {
            closeButton.onClick.RemoveListener(CompleteTutorial);

            currentBehaviour?.DeinitializeButtons();
        }

        #endregion



        #region Events handlers

        public void CompleteTutorial()
        {
            completeTutorialCallback?.Invoke();

            Hide();
        }

        #endregion
    }
}
