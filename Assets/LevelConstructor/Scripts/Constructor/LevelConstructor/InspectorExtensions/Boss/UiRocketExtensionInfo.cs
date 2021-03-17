using System;
using System.Collections.Generic;
using Drawmasters.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class UiRocketExtensionInfo : MonoBehaviour
    {
        #region Fields

        public static event Action<EnemyBossExtensionRocketDraw.Data> OnShouldRedraw;
        public static event Action<EnemyBossExtensionRocketDraw.Data> OnShouldRemove;
        public static event Action<EnemyBossExtensionRocketDraw.Data, ShooterColorType> OnShouldChangeColorType;

        [SerializeField] private Button removeButton = default;
        [SerializeField] private TMP_Text removeButtonText = default;

        [SerializeField] private Button redrawButton = default;
        [SerializeField] private TMP_Text redrawButtonText = default;
        [SerializeField] private OptionsChoiceUI colorChoiseOption = default;

        private EnemyBossExtensionRocketDraw.Data data;
        private ShooterColorType colorType;

        #endregion


        #region Methods

        public void Initialize(EnemyBossExtensionRocketDraw.Data _data)
        {
            data = _data;

            List<string> availableTypes = new List<string>(Enum.GetNames(typeof(ShooterColorType)));
            colorChoiseOption.Init("Rocket Colors", availableTypes, default);

            removeButton.onClick.AddListener(RemoveButton_OnClick);
            redrawButton.onClick.AddListener(RedrawButton_OnClick);
            colorChoiseOption.OnValueChanged += ColorChoiseOption_OnValueChanged;
        }

        public void RefreshIndex(int index)
        {
            List<string> availableTypes = new List<string>(Enum.GetNames(typeof(ShooterColorType)));
            colorChoiseOption.Init($"Rocket Color #{index}", availableTypes, (int)data.shooterColorType);

            redrawButtonText.text = $"Redraw #{index}";
            removeButtonText.text = $"Remove #{index}";
        }


        public void Deinitialize()
        {
            removeButton.onClick.RemoveListener(RemoveButton_OnClick);
            redrawButton.onClick.RemoveListener(RedrawButton_OnClick);
            colorChoiseOption.OnValueChanged -= ColorChoiseOption_OnValueChanged;

            data = null;
        }

        public void SetupColorType(ShooterColorType _colorType)
        {
            colorType = _colorType;
            colorChoiseOption.SetValue((int)colorType);
        }

        #endregion



        #region Events handlers

        private void RemoveButton_OnClick() =>
            OnShouldRemove?.Invoke(data);
        

        private void RedrawButton_OnClick() =>
            OnShouldRedraw?.Invoke(data);
        

        private void ColorChoiseOption_OnValueChanged(int value)
        {
            ShooterColorType colorType = (ShooterColorType)value;
            OnShouldChangeColorType?.Invoke(data, colorType);
        }

        #endregion
    }
}
