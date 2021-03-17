using Drawmasters.Tutorial;
using System;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;


namespace Drawmasters.Ui
{
    public class TutorialScreen : AnimatorScreen, ITutorialScreen
    {
        #region Fields

        private const int AnimationIndex = 0;

        [SerializeField] private Button nextButton = default;
        [SerializeField] private SkeletonGraphic skeletonGraphic = default;
        [SerializeField] private TutorialButton tutorialButton = default;

        private Action nextButtonPressedCallback;

        #endregion



        #region Properties

        public override ScreenType ScreenType => ScreenType.Tutorial;

        #endregion



        #region Methods

        public void Initialize(TutorialType type, Action completeCallback)
        {
            TutorialSettings.GameTutorialData data = IngameData.Settings.tutorialSettings.FindGameData(type);

            if (data == null)
            {
                return;
            }

            skeletonGraphic.skeletonDataAsset = data.animationDataAsset;
            skeletonGraphic.Initialize(true);
            skeletonGraphic.AnimationState.SetAnimation(AnimationIndex, data.startAnimation.ToString(), true);

            tutorialButton.Initialize(data.buttonEnableDelay);

            nextButtonPressedCallback = completeCallback;
        }


        public override void Deinitialize()
        {
            tutorialButton.Deinitialize();

            base.Deinitialize();
        }

        #endregion



        #region Overrided methods

        public override void Show()
        {
            base.Show();

            nextButtonPressedCallback = null;
        }


        public override void InitializeButtons()
        {
            nextButton.onClick.AddListener(NextButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            nextButton.onClick.RemoveListener(NextButton_OnClick);
        }

        #endregion



        #region Events handlers

        private void NextButton_OnClick()
        {
            Hide(view => nextButtonPressedCallback?.Invoke(), null);
        }

        #endregion
    }
}
