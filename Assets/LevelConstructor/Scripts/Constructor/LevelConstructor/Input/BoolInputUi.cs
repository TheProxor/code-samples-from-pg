using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class BoolInputUi : MonoBehaviour
    {
        #region Fields

        public event Action<bool> OnValueChange;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private Toggle toggle = default;

        #endregion



        #region Unity lifecycle

        private void Awake()
        {
            toggle.onValueChanged.AddListener((bool value) => { OnValueChange?.Invoke(value); });
        }

        #endregion



        #region Methods

        public void Init(string title, bool startingValue)
        {
            titleLabel.text = title;
            toggle.isOn = startingValue;
        }

        #endregion
    }
}
