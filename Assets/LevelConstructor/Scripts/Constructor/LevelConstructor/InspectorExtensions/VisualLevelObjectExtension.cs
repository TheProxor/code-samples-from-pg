using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class VisualLevelObjectExtension : InspectorExtensionBase
    {
        #region Fields

        [SerializeField] private SliderInputUi spriteChangeSlider = default;
        [SerializeField] private IntInputUi sortingOrderInput = default;

        private EditorVisualLevelObject visualLevelObject;

        private Sprite[] availableSprites;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            visualLevelObject = levelObject as EditorVisualLevelObject;

            if (visualLevelObject != null)
            {
                availableSprites = IngameData.Settings.commonVisualObjectsSettings.FindSprites(EditorObjectsContainer.CurrentWeaponType);

                spriteChangeSlider.MarkWholeNumbersOnly();
                spriteChangeSlider.Init("Sprite", visualLevelObject.SpriteIndex, 0.0f, availableSprites.Length - 1.0f);

                sortingOrderInput.Init("Sorting order", visualLevelObject.SortingOrder);

                RefreshObjectSprite();
            }
        }


        protected override void SubscribeOnEvents()
        {
            spriteChangeSlider.OnValueChange += SpriteChangeSlider_OnValueChange;
            sortingOrderInput.OnValueChange += SortingOrderInput_OnValueChange;
        }


        protected override void UnsubscribeFromEvents()
        {
            spriteChangeSlider.OnValueChange -= SpriteChangeSlider_OnValueChange;
            sortingOrderInput.OnValueChange -= SortingOrderInput_OnValueChange;
        }


        private void RefreshObjectSprite()
        {
            visualLevelObject.Refresh(availableSprites[visualLevelObject.SpriteIndex]);
        }

        #endregion



        #region Events handlers

        private void SpriteChangeSlider_OnValueChange(float value)
        {
            int spriteIndex = Mathf.CeilToInt(value);

            visualLevelObject.SpriteIndex = spriteIndex;
            RefreshObjectSprite();
        }


        private void SortingOrderInput_OnValueChange(int value)
        {
            visualLevelObject.SortingOrder = value;
            RefreshObjectSprite();
        }

        #endregion
    }
}
