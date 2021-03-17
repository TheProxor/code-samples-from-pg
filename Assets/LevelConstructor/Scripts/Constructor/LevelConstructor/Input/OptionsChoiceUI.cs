using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class OptionsChoiceUI : MonoBehaviour
    {
        #region Fields

        public event Action<int> OnValueChanged;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private TMP_Dropdown dropdown = default;

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            dropdown.onValueChanged.AddListener((newValue) => OnValueChanged?.Invoke(newValue));
        }

        #endregion



        #region Methods

        public void Init(string title, List<string> options, int value)
        {
            titleLabel.text = title;

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = value;
        }

        public void SetValue(int value) =>
            dropdown.value = value;
        
        #endregion
    }
}
