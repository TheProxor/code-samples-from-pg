using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using System;
using System.Collections.Generic;


namespace Drawmasters.Tutorial
{
    public static class TutorialManager
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            public List<TutorialType> completedTutorials = new List<TutorialType>();
        }

        #endregion



        #region Fields

        public static event Action<TutorialType> OnTutorEnded;

        private static readonly Data data;

        private static Action finishTutorialCallback;

        #endregion



        #region Properties

        public static TutorialType DrawTutorialType =>
            GameServices.Instance.LevelOrderService.ShouldLoadAlternativeLevelsPack ? TutorialType.DrawAlternative : TutorialType.Draw;


        public static bool WasTutorialCompleted(TutorialType type) =>
            data.completedTutorials.Contains(type);


        // Obsolete
        private static bool WasDrawTutorial =>
            CustomPlayerPrefs.GetBool(PrefsKeys.Tutorial.MainDrawTutorial, false);
            
        #endregion



        #region Class lifecycle

        static TutorialManager()
        {
            data = CustomPlayerPrefs.GetObjectValue<Data>(PrefsKeys.Tutorial.Data);

            if (data == null)
            {
                data = new Data();
                SaveData();
            }

            // Old users logic
            if (WasDrawTutorial)
            {
                data.completedTutorials.Add(TutorialType.Draw);
                data.completedTutorials.Add(TutorialType.DrawAlternative);
                SaveData();
            }
        }

        #endregion



        #region Methods
        
        public static TutorialType FindGameTutorial(GameMode mode) =>
            IngameData.Settings.tutorialSettings.FindModeTutorialType(mode);
        

        public static void StartTurorial(TutorialType type, ScreenType screenType, Action finishCallback)
        {
            finishTutorialCallback = finishCallback;

            AnimatorScreen view = UiScreenManager.Instance.ShowScreen(screenType);

            if (view is ITutorialScreen tutorialScreen)
            {
                tutorialScreen.Initialize(type, () => CompleteTutorial(type));
            }
            else
            {
                CustomDebug.Log($"Screen {screenType} doesn't have ITutorialScreen implementation");
            }
        }


        private static void CompleteTutorial(TutorialType type)
        {
            data.completedTutorials.Add(type);
            SaveData();

            finishTutorialCallback?.Invoke();
            finishTutorialCallback = null;

            OnTutorEnded?.Invoke(type);
        }


        private static void SaveData() =>
            CustomPlayerPrefs.SetObjectValue(PrefsKeys.Tutorial.Data, data);

        #endregion
    }
}
