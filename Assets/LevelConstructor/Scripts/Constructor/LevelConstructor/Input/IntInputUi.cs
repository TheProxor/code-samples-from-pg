using UnityEngine;
using TMPro;
using System;


namespace Drawmasters.LevelConstructor
{
    public class IntInputUi : MonoBehaviour
    {
        #region Fields

        public event Action<int> OnValueChange;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private TMP_InputField inputField = default;

        private int minValue;
        private int maxValue;

        #endregion



        #region Properties

        public int Value { get; private set; }

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

        public void Init(string title, int startingValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            titleLabel.text = title;
            Value = startingValue;
            SetValue(Value);

            this.minValue = minValue;
            this.maxValue = maxValue;
        }


        private void SetValue(int value)
        {
            inputField.text = value.ToString();
        }

        #endregion



        #region Events handlers

        private void Input_OnValueChange(string value)
        {
            if (int.TryParse(value, out int newValue))
            {
                Value = Mathf.Clamp(newValue, minValue, maxValue);
                SetValue(Value);
                OnValueChange?.Invoke(Value);
            }
            else
            {
                SetValue(Value);
            }
        }

        #endregion
    }
}
