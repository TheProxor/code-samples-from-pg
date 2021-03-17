using System;
using UnityEngine;
using Drawmasters.Levels;


namespace Drawmasters.LevelConstructor
{
    public class LiquidLevelObjectExtension : InspectorExtensionBase
    {
        #region Fields

        [SerializeField] private FloatInputUi widthChangeInput = default;
        [SerializeField] private FloatInputUi heightChangeInput = default;

        [SerializeField] private SliderInputUi typeChangeSlider = default;

        private EditorLiquidLevelObject editorObject;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            editorObject = levelObject as EditorLiquidLevelObject;

            if (editorObject != null)
            {
                typeChangeSlider.MarkWholeNumbersOnly();
                typeChangeSlider.Init(editorObject.Type.ToString(), (int)editorObject.Type, 1.0f, Enum.GetValues(typeof(LiquidLevelObjectType)).Length - 1);
                widthChangeInput.Init("Width", editorObject.Size.x, 1.0f);
                heightChangeInput.Init("Height", editorObject.Size.y, 1.0f);
            }
        }


        protected override void SubscribeOnEvents()
        {
            typeChangeSlider.OnValueChange += TypeChangeSlider_OnValueChange;
            widthChangeInput.OnValueChange += WidthChangeInput_OnValueChange;
            heightChangeInput.OnValueChange += HeightChangeInput_OnValueChange;
        }


        protected override void UnsubscribeFromEvents()
        {
            typeChangeSlider.OnValueChange -= TypeChangeSlider_OnValueChange;
            widthChangeInput.OnValueChange -= WidthChangeInput_OnValueChange;
            heightChangeInput.OnValueChange -= HeightChangeInput_OnValueChange;
        }

        #endregion



        #region Events handlers

        private void TypeChangeSlider_OnValueChange(float value)
        {
            value = Mathf.CeilToInt(value);
            LiquidLevelObjectType enumValue = (LiquidLevelObjectType)Enum.ToObject(typeof(LiquidLevelObjectType), (int)value);
            editorObject.Type = enumValue;

            typeChangeSlider.SetTitle(enumValue.ToString());

            editorObject.RefreshData();
        }


        private void WidthChangeInput_OnValueChange(float value)
        {
            editorObject.Size = editorObject.Size.SetX(value);
            editorObject.RefreshData();
        }


        private void HeightChangeInput_OnValueChange(float value)
        {
            editorObject.Size = editorObject.Size.SetY(value);
            editorObject.RefreshData();
        }

        #endregion
    }
}
