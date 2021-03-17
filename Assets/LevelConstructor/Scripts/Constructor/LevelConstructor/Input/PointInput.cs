using Drawmasters.Levels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class PointInput : MonoBehaviour
    {
        #region Nested Types

        [Serializable]
        private class TargetModeButtonInfo
        {
            public Button button = default;
            public Image buttonImage = default;
            public TextMeshProUGUI buttonName = default;
            public ShapeTangentMode buttonTangentMode = default;
        }


        [Serializable]
        private class SelectableColorInfo
        {
            public Color selectedButtonImage = default;
            public Color defaultButtonColor = default;
        }

        #endregion



        #region Fields

        public static event Action<int, ShapeTangentMode> OnTargetModeButtonClick;
        public static event Action<int, Vector3> OnPointPositionChange;

        private const string TitleLabelTitle = "Index:";
        private const string PointPositionTitle = "Position:";
        private const int PointPositionFractionalDigitsCount = 2;

        [SerializeField] private TextMeshProUGUI titleLabel = default;
        [SerializeField] private Vector3UI pointPositionHandler = default;

        [Header("Buttons")]
        [SerializeField] private List<TargetModeButtonInfo> targetModeButtonInfos = default;
        [SerializeField] private SelectableColorInfo selectableButtonsColorInfo = default;

        [Header("Background")]
        [SerializeField] private Image backgroundImage = default;
        [SerializeField] private SelectableColorInfo backgroundImageColorInfo = default;

        private int index = default;
        private ShapeTangentMode shapeTangentMode = default;

        #endregion



        #region Properties

        public ShapeTangentMode ShapeTangentMode
        {
            get => shapeTangentMode;
            set
            {
                shapeTangentMode = value;

                foreach (TargetModeButtonInfo buttonInfo in targetModeButtonInfos)
                {
                    bool isButtonSelected = buttonInfo.buttonTangentMode == shapeTangentMode;
                    buttonInfo.buttonImage.color = (isButtonSelected) ?
                        selectableButtonsColorInfo.selectedButtonImage :
                        selectableButtonsColorInfo.defaultButtonColor;
                }
            }
        }

        #endregion



        #region Unity lifecycle

        private void OnEnable()
        {
            foreach (TargetModeButtonInfo buttonInfo in targetModeButtonInfos)
            {
                buttonInfo.button.onClick.AddListener(() => TargetModeButton_OnClick(buttonInfo.buttonTangentMode));
                buttonInfo.buttonName.text = buttonInfo.buttonTangentMode.ToString();
            }

            pointPositionHandler.OnValueChange += PointPositionHandler_OnValueChange;
        }


        private void OnDisable()
        {
            foreach (TargetModeButtonInfo buttonInfo in targetModeButtonInfos)
            {
                buttonInfo.button.onClick.RemoveAllListeners();
            }

            pointPositionHandler.OnValueChange -= PointPositionHandler_OnValueChange;
        }

        #endregion



        #region Methods

        public void Initialize(PointData pointData)
        {
            index = pointData.pointIndex;

            pointPositionHandler.Init(PointPositionTitle, PointPositionFractionalDigitsCount);
            pointPositionHandler.SetCurrentValue(pointData.pointPosition);

            ShapeTangentMode = pointData.shapeTangentMode;
            SetSelected(false);

            titleLabel.text = $"{TitleLabelTitle} {index}";
        }


        public void UpdatePointData(PointData pointData)
        {
            pointPositionHandler.SetCurrentValue(pointData.pointPosition);
            ShapeTangentMode = pointData.shapeTangentMode;
        }


        public void SetSelected(bool isSelected)
        {
            backgroundImage.color = (isSelected) ?
                backgroundImageColorInfo.selectedButtonImage :
                backgroundImageColorInfo.defaultButtonColor;
        }

        #endregion



        #region Events handlers

        private void TargetModeButton_OnClick(ShapeTangentMode shapeTangentMode)
        {
            OnTargetModeButtonClick?.Invoke(index, shapeTangentMode);
        }


        private void PointPositionHandler_OnValueChange(Vector3 newPosition)
        {
            OnPointPositionChange?.Invoke(index, newPosition);
        }

        #endregion
    }
}
