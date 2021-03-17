using System;


namespace Drawmasters.Ui
{
    public partial class DialogPopupSettings
    {
        #region Helpers

        [Serializable]
        public class DialogSettingsContainer
        {
            public OkPopupType dialogPopupType = default;
            public string headerText = default;
            public string contentText = default;
        }

        #endregion
    }
}