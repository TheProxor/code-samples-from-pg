using System;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class Float3InputRemovableUi : Float3InputUi
    {
        #region Fields

        public event Action<Float3InputRemovableUi, Vector3> OnValueChanged;
        public event Action<Float3InputRemovableUi> OnRemoved;

        [SerializeField] private Button removeButton = default;
        [SerializeField] private LayoutElement layoutElement = default;

        #endregion



        #region Properties

        public float Height => layoutElement.preferredHeight;

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            OnChangeX += OnChangedValue;
            OnChangeY += OnChangedValue;
            OnChangeZ += OnChangedValue;

            removeButton.onClick.AddListener(RemoveButton_OnClick);
        }


        private void OnDisable()
        {
            OnChangeX -= OnChangedValue;
            OnChangeY -= OnChangedValue;
            OnChangeZ -= OnChangedValue;

            removeButton.onClick.RemoveListener(RemoveButton_OnClick);
        }

        #endregion


        #region Methods

        public void SetCurrentValue(Vector3 position)
        {
            SetCurrentValue(position.x, 
                            position.y,
                            position.z);
        }

        #endregion



        #region Events handlers

        private void OnChangedValue(float changedValue)
        {
            OnValueChanged?.Invoke(this, currentValue);
        }


        private void RemoveButton_OnClick()
        {
            OnRemoved?.Invoke(this);
        }

        #endregion
    }
}
