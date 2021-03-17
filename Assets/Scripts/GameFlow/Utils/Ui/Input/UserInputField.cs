using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


namespace Drawmasters.Utils.Ui
{
    public class UserInputField : MonoBehaviour
    {
        #region Fields

        public event Action<string> OnInputSubmited;

        [SerializeField] private GameObject root = default;
        [SerializeField] private TMP_InputField inputField = default;
        [SerializeField] [Tooltip("Optional")] private TMP_Text errorMessageText = default;
        [SerializeField] [Tooltip("Optional")] private Button submitButton = default;

        private string startSelectedText;

        #endregion



        #region Methods

        public void Initialize()
        {
            inputField.onSelect.AddListener(InputField_OnSelect);
            inputField.onSubmit.AddListener(InputField_OnSubmit);

            if (submitButton != null)
            {
                submitButton.onClick.AddListener(SubmitButton_OnClick);
            }

            SetErrorMessage(string.Empty);
        }


        public void Deinitialize()
        {
            inputField.onSelect.RemoveListener(InputField_OnSelect);
            inputField.onSubmit.RemoveListener(InputField_OnSubmit);

            if (submitButton != null)
            {
                submitButton.onClick.RemoveListener(SubmitButton_OnClick);
            }
        }


        public void SetRootEnabled(bool enabled) =>
            CommonUtility.SetObjectActive(root, enabled);


        public void SetText(string text) =>
            inputField.text = text;

        
        public void SetSubmitText(string text)
        {
            SetText(text);
            SubmitInput(text);
        }
        

        private void SubmitInput(string text)
        {
            bool isValid = ValidateText(text, out string errorMessage);
            SetErrorMessage(errorMessage);

            if (!isValid)
            {
                SetText(startSelectedText);
                return;
            }

            OnInputSubmited?.Invoke(text);
        }


        private bool ValidateText(string text, out string errorMessage)
        {
            bool result = true;
            errorMessage = string.Empty;


            if (string.IsNullOrEmpty(text))
            {
                result = false;
                errorMessage = "Field couldn't be empty!";
            }

            return result;
        }


        private void SetErrorMessage(string errorMessage)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = errorMessage;
            }
        }

        #endregion



        #region Events handlers

        private void InputField_OnSelect(string text)
        {
            SetErrorMessage(string.Empty);
            startSelectedText = text;
        }


        private void InputField_OnSubmit(string text) =>
            SubmitInput(text);
        

        private void SubmitButton_OnClick() =>
            SubmitInput(inputField.text);
        
        #endregion
    }
}
