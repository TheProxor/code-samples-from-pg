using System;
using Drawmasters.Tutorial;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class FingerTutorialLevelLoader : ILevelLoader
    {
        #region Fields

        private readonly TutorialType tutorialType;
        private readonly ScreenType screenType;
        private Action onLoaded;

        #endregion



        #region Class lifecycle

        public FingerTutorialLevelLoader(TutorialType _tutorialType, ScreenType _screenType)
        {
            tutorialType = _tutorialType;
            screenType = _screenType;
        }

        #endregion



        #region ILevelLoader

        public void LoadLevel(Action _onLoaded)
        {
            onLoaded = _onLoaded;

            UiScreenManager.Instance.ShowScreen(ScreenType.Ingame, view =>
            {
                TutorialManager.StartTurorial(tutorialType, screenType, null);
                AnimatorScreen screen = UiScreenManager.Instance.LoadedScreen<AnimatorScreen>(screenType);

                if (screen != null)
                {
                    screen.OnShowEnd += Screen_OnShowEnd;
                }
                else
                {
                    onLoaded?.Invoke();
                }
            });
        }


        private void Screen_OnShowEnd(AnimatorView view)
        {
            view.OnShowEnd -= Screen_OnShowEnd;

            onLoaded?.Invoke();
        }

        #endregion
    }
}
