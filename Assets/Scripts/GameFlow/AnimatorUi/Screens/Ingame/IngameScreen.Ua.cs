using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Drawmasters.Ui
{
    public partial class IngameScreen
    {
        #region Fields

        private static bool isUiEnabled = true;

        #endregion



        #region Properties

        public static bool IsUiEnabled
        {
            get => isUiEnabled;
            set
            {
                isUiEnabled = value;

                IngameScreen screen = UiScreenManager.Instance.LoadedScreen<IngameScreen>(ScreenType.Ingame);

                if (screen != null)
                {
                    IngameScreen.ChangeUiVisabilty(screen.gameObject, isUiEnabled);
                }
            }
        }

        #endregion



        #region Methods

        //TODO:
        //Using the current approach: when switching the option UI/UX in the cheat menu, 
        //the transparency of ALL objects becomes equal to zero or one, 
        //which creates a bug for transparent objects(for example, with an alpha in color equal to 0.75)
        public static void ChangeUiVisabilty(GameObject screenObject, bool isEnabled)
        {
            float alpha = (isEnabled) ? (1f) : (0f);
            
            List<Image> images = new List<Image>(screenObject.GetComponentsInChildren<Image>(true));
            images.ForEach(im => im.color = im.color.SetA(alpha));

            List<TextMeshProUGUI> tmproText = new List<TextMeshProUGUI>(screenObject.GetComponentsInChildren<TextMeshProUGUI>(true));
            tmproText.ForEach(text => text.color = text.color.SetA(alpha));

            List<Text> uiText = new List<Text>(screenObject.GetComponentsInChildren<Text>(true));
            uiText.ForEach(text => text.color = text.color.SetA(alpha));
        }

        #endregion
    }
}
