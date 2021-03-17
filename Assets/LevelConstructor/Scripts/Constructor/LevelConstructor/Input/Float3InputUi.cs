using System;
using TMPro;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class Float3InputUi : MonoBehaviour
    {
        #region Fields

        const string UnusedText = "-";

        public event Action<float> OnChangeX;
        public event Action<float> OnChangeY;
        public event Action<float> OnChangeZ;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private TMP_InputField inputX = default;
        [SerializeField] private TMP_InputField inputY = default;
        [SerializeField] private TMP_InputField inputZ = default;
        [SerializeField] private Color defaultSelectionColor = default;
        [SerializeField] private Color disabledSelectionColor = Color.red;

        private int fractionalDigits;

        protected Vector3 currentValue = Vector3.zero;

        #endregion



        #region Unity lifecycle

        private void Start()
        {
            inputX.onEndEdit.AddListener(OnInputChangeX);
            inputY.onEndEdit.AddListener(OnInputChangeY);
            inputZ.onEndEdit.AddListener(OnInputChangeZ);
        }

        #endregion



        #region Methods

        public void Init(string title, int fractionalDigits)
        {
            titleLabel.text = title;

            this.fractionalDigits = fractionalDigits;

            SetCurrentValue(0.0f, 0.0f, 0.0f);
        }


        public void SetCurrentValue(float? x, float? y, float? z)
        {
            SetValue(x, inputX, (value) => currentValue.x = value);
            SetValue(y, inputY, (value) => currentValue.y = value);
            SetValue(z, inputZ, (value) => currentValue.z = value);
        }


        public void UpdateButtonsVisibility(Vector3 visibilityControl)
        {
            UpdateInputAvailability(inputX, visibilityControl.x > 0.0f);
            UpdateInputAvailability(inputY, visibilityControl.y > 0.0f);
            UpdateInputAvailability(inputZ, visibilityControl.z > 0.0f);
        }


        private void UpdateInputAvailability(TMP_InputField inputField, bool isAvailable)
        {
            inputField.readOnly = !isAvailable;
            inputField.selectionColor = (isAvailable) ? defaultSelectionColor : disabledSelectionColor;
        }


        private void SetValue(float? value, TMP_InputField inputField, Action<float> setValueDelegate)
        {
            if (value.HasValue)
            {
                UpdateInputValue(inputField, value.Value);
                setValueDelegate(value.Value);
            }
            else
            {
                UpdateInputText(inputField, UnusedText);
                setValueDelegate(float.MinValue);
            }
        }


        private void UpdateInputValue(TMP_InputField inputField, float value)
        {
            UpdateInputText(inputField, Math.Round(value, fractionalDigits).ToString());
        }


        private void UpdateInputText(TMP_InputField inputField, string text)
        {
            if (!inputField.isFocused)
            {
                inputField.text = text;
            }
        }

        #endregion



        #region Events handlers

        private void OnInputChangeX(string value)
        {
            if (!float.TryParse(value, out float result))
            {
                result = 0f;
            }

            currentValue.x = result;
            UpdateInputValue(inputX, result);

            OnChangeX?.Invoke(result);
        }


        private void OnInputChangeY(string value)
        {
            if (!float.TryParse(value, out float result))
            {
                result = 0f;
            }

            currentValue.y = result;
            UpdateInputValue(inputY, result);

            OnChangeY?.Invoke(result);
        }


        private void OnInputChangeZ(string value)
        {
            if (!float.TryParse(value, out float result))
            {
                result = 0f;
            }

            currentValue.z = result;
            UpdateInputValue(inputZ, result);

            OnChangeZ?.Invoke(result);
        }

        #endregion
    }
}
