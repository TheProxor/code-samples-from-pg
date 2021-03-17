using System;
using Drawmasters.Tutorial;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class TutorialLevelLoader : ILevelLoader
    {
        #region Fields

        private readonly TutorialType tutorType;

        private Action onLevelLoaded;

        #endregion



        #region Ctor

        public TutorialLevelLoader(TutorialType _tutorType, Action runTutorial)
        {
            tutorType = _tutorType;

            TutorialManager.OnTutorEnded += TutorialManager_OnTutorEnded;

            runTutorial?.Invoke();

            TutorialManager.StartTurorial(tutorType, ScreenType.Tutorial, null);
        }



        #endregion



        #region ILevelLoader

        public void LoadLevel(Action onLoaded)
        {
            onLevelLoaded = onLoaded;
        }

        #endregion



        #region Events handlers

        private void TutorialManager_OnTutorEnded(TutorialType completedTutorial)
        {
            if (tutorType == completedTutorial)
            {
                TutorialManager.OnTutorEnded -= TutorialManager_OnTutorEnded;

                UiScreenManager.Instance.ShowScreen(ScreenType.Ingame);

                onLevelLoaded?.Invoke();
            }
        }

        #endregion
    }
}