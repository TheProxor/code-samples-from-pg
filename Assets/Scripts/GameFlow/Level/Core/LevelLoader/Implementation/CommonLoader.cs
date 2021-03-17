using System;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class CommonLoader : ILevelLoader
    {
        #region ILevelLoader

        public void LoadLevel(Action onLoaded)
        {
            UiScreenManager.Instance.ShowScreen(ScreenType.Ingame, view => onLoaded?.Invoke());
        }

        #endregion
    }
}