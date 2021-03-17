using System;
using System.Collections.Generic;
using Drawmasters.Levels;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class ResultPauseBehaviour : IResultBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            [Required] public GameObject rootObject = default;
            [Required] public GameObject headerRootObject = default;
            [Required] public Button menuButton = default;
            [Required] public Button resumeButton = default;
        }

        #endregion
        
        
        
        #region Fields

        private readonly List<GameObject> objects;
        private readonly Data data;
        private readonly ResultScreen resultScreen;

        #endregion



        #region Ctor

        public ResultPauseBehaviour(Data _data, ResultScreen screen)
        {
            data = _data;
            objects = new List<GameObject> { data.rootObject, data.headerRootObject };
            resultScreen = screen;
        }

        #endregion


        #region IDeinitializable

        public void Deinitialize() { }

        #endregion



        #region IResultBehaviour

        public void Enable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, true));
            
            data.resumeButton.onClick.AddListener(ResumeButton_OnClick);
            data.menuButton.onClick.AddListener(MenuButton_OnClick);
        }

        public void Disable()
        {
            objects.ForEach(go => CommonUtility.SetObjectActive(go, false));
            DeinitializeButtons();
        }


        public void InitializeButtons() { }


        public void DeinitializeButtons()
        {
            data.resumeButton.onClick.RemoveListener(ResumeButton_OnClick);
            data.menuButton.onClick.RemoveListener(MenuButton_OnClick);
        }

        #endregion



        #region Events hanlers

        private void ResumeButton_OnClick() => resultScreen.Hide();
        
        private void MenuButton_OnClick()
        {
            data.resumeButton.onClick.RemoveListener(ResumeButton_OnClick);
            data.menuButton.onClick.RemoveListener(MenuButton_OnClick);

            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);
                
                LevelsManager.Instance.CompleteLevel(LevelResult.Leave);
            });
        }

        #endregion
    }
}
