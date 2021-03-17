using UnityEngine;
using TMPro;
using System;


namespace Drawmasters.LevelConstructor
{
    public class FloatInputUi : MonoBehaviour
    {
        #region Fields

        public event Action<float> OnValueChange;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private TMP_InputField inputField = default;

        private float minValue;
        private float maxValue;

        #endregion



        #region Properties

        public float Value { get; private set; }

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            inputField.onEndEdit.AddListener(Input_OnValueChange);
        }


        private void OnDestroy()
        {
            inputField.onEndEdit.RemoveListener(Input_OnValueChange);
        }

        #endregion



        #region Public methods

        public void Init(string title, float startingValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            titleLabel.text = title;

            this.minValue = minValue;
            this.maxValue = maxValue;

            SetValue(startingValue);
        }


        private void SetValue(float newValue)
        {
            Value = Mathf.Clamp(newValue, minValue, maxValue);

            inputField.text = Value.ToString();
        }

        #endregion



        #region Events handlers

        private void Input_OnValueChange(string value)
        {
            if (float.TryParse(value, out float newValue))
            {
                SetValue(newValue);
                OnValueChange?.Invoke(Value);
            }
        }

        #endregion
    }
}
