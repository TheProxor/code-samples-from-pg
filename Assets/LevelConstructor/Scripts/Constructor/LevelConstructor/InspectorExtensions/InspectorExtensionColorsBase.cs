using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class InspectorExtensionColorsBase : InspectorExtensionBase
    {
        #region Fields

        [SerializeField] private OptionsChoiceUI choiceColorType = default;

        private EditorColorsLevelObject levelObject;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject _levelObject)
        {
            levelObject = _levelObject as EditorColorsLevelObject;

            if (levelObject != null)
            {
                List<string> availableTypes = new List<string>(Enum.GetNames(typeof(ShooterColorType)));
                int currentIndex = (int)levelObject.ColorType;
                choiceColorType.Init("Color", availableTypes, currentIndex);

                RefreshVisual();
            }
            else
            {
                Debug.Log($"levelObject as EditorColorsLevelObject is NULL. Are EditorLevelObject iheritance missed?");
            }
        }


        protected override void SubscribeOnEvents()
        {
            choiceColorType.OnValueChanged += SetColorType;
        }


        protected override void UnsubscribeFromEvents()
        {
            choiceColorType.OnValueChanged -= SetColorType;
        }


        private void RefreshVisual()
        {
            levelObject.Refresh();
        }

        #endregion



        #region Events handlers


        private void SetColorType(int index)
        {
            levelObject.ColorType = (ShooterColorType)index;
            RefreshVisual();
        }

        #endregion
    }
}
