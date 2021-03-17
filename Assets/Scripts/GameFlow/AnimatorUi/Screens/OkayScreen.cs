using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class OkayScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private TMP_Text headerTextField = default;
        [SerializeField] private TMP_Text textField = default;

        [SerializeField] private Button okayButton = default;
        [SerializeField] private Button closeButton = default;

        #endregion 



        #region Overrided properties

        public override ScreenType ScreenType => ScreenType.OkayScreen;

        #endregion



        #region Overrided methods

        public override void DeinitializeButtons()
        {
            okayButton.onClick.RemoveListener(Hide);
            closeButton.onClick.RemoveListener(Hide);
        }


        public override void InitializeButtons()
        {
            okayButton.onClick.AddListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        #endregion



        #region Methods

        public void ChangeText(string text)
        {
            text = text.Replace("\\n", "\n");
            textField.SetText(text);
        }


        public void ChangeHeader(string text) =>
            headerTextField.text = text;

        #endregion
    }
}

