using System;
using TMPro;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class Vector3UI : MonoBehaviour
    {
        #region Fields

        public event Action<Vector3> OnValueChange = delegate { };

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private TMP_InputField inputX = default;
        [SerializeField] private TMP_InputField inputY = default;
        [SerializeField] private TMP_InputField inputZ = default;
        [SerializeField] private Color defaultSelectionColor = default;
        [SerializeField] private Color disabledSelectionColor = default;

        private int fractionalDigits;

        #endregion



        #region Properties

        public Vector3 CurrentValue { get; private set; }

        #endregion



        #region Unity lifecycle

        private void Start()
        {
            inputX.onEndEdit.AddListener((string value) => OnInputChange(value, true, false, false));
            inputY.onEndEdit.AddListener((string value) => OnInputChange(value, false, true, false));
            inputZ.onEndEdit.AddListener((string value) => OnInputChange(value, false, false, true));
        }

        #endregion



        #region Methods

        public void Init(string title, int fractionalDigits)
        {
            titleLabel.text = title;
            
            this.fractionalDigits = fractionalDigits;
            
            SetCurrentValue(Vector3.zero);
        }


        public void SetCurrentValue(Vector3 newValue)
        {
            CurrentValue = newValue;

            UpdateInputValue(inputX, CurrentValue.x);
            UpdateInputValue(inputY, CurrentValue.y);
            UpdateInputValue(inputZ, CurrentValue.z);
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
        

        private void UpdateInputValue(TMP_InputField inputField, float value)
        {
            if (!inputField.isFocused)
            {
                inputField.text = Math.Round(value, fractionalDigits).ToString();
            }
        }

        #endregion



        #region Events handlers

        private void OnInputChange(string value, bool changeX, bool changeY, bool changeZ)
        {
            if (!float.TryParse(value, out float result))
            {
                result = 0f;
            }

            Vector3 newValue = CurrentValue;
            newValue.x = changeX ? result : newValue.x;
            newValue.y = changeY ? result : newValue.y;
            newValue.z = changeZ ? result : newValue.z;

            SetCurrentValue(newValue);

            OnValueChange(newValue);
        }

        #endregion
    }
}
