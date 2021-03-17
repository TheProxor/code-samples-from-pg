using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class SliderInputUi : MonoBehaviour
    {
        #region Fields

        public event Action<float> OnValueChange;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private Slider slider = default;

        #endregion



        #region Properties

        public float CurrentValue => slider.value;

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            slider.onValueChanged.AddListener(Slider_OnValueChange);
        }


        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(Slider_OnValueChange);
        }

        #endregion



        #region Public methods

        public void Init(string title, float startingValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        {
            SetTitle(title);

            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = startingValue;
        }


        public void SetTitle(string value)
        {
            titleLabel.text = value;
        }


        public void MarkWholeNumbersOnly()
        {
            slider.wholeNumbers = true;
        }


        public void RemoveAllListeners()
        {
            OnValueChange = null;
        }

        #endregion



        #region Events handlers

        private void Slider_OnValueChange(float value)
        {
            OnValueChange?.Invoke(value);
        }

        #endregion
    }
}
