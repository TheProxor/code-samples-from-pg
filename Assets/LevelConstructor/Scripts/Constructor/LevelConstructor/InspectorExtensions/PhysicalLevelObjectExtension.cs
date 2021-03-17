using Drawmasters.Levels;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class PhysicalLevelObjectExtension : InspectorExtensionBase
    {
        #region Fields

        [SerializeField] private LayoutElement mainLayoutElement = default;
        [SerializeField] private SliderInputUi spriteChangeSlider = default;
        [SerializeField] private JointInterface jointInterface = default;
        
        private EditorPhysicalLevelObject physicalObject;

        private Sprite[] availableSprites;
        private float? initialHeight;

        #endregion


        #region Properties

        private float InitialHeight
        {
            get
            {
                if (initialHeight == null)
                {
                    initialHeight = mainLayoutElement.preferredHeight;
                }

                return initialHeight.Value;
            }
        }

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            PhysicalLevelObject referenceObject = Content.Storage.GetLevelObject(levelObject.Index) as PhysicalLevelObject;
            availableSprites = IngameData.Settings.physicalObject.GetSprites(referenceObject.PhysicalData);

            physicalObject = levelObject as EditorPhysicalLevelObject;

            if (physicalObject != null)
            {
                spriteChangeSlider.MarkWholeNumbersOnly();
                spriteChangeSlider.Init("Sprite", physicalObject.SpriteIndex, 0.0f, availableSprites.Length - 1.0f);

                jointInterface.Init(levelObject);

                RefreshObjectSprite();
            }
        }


        protected override void SubscribeOnEvents()
        {
            spriteChangeSlider.OnValueChange += SpriteChangeSlider_OnValueChange;
            jointInterface.OnHeightChanged += OnHeightChanged;
        }


        protected override void UnsubscribeFromEvents()
        {
            spriteChangeSlider.OnValueChange -= SpriteChangeSlider_OnValueChange;
            jointInterface.OnHeightChanged -= OnHeightChanged;
        }


        private void RefreshObjectSprite()
        {
            physicalObject.SetSprite(availableSprites[physicalObject.SpriteIndex]);
        }

        #endregion



        #region Events handlers

        private void SpriteChangeSlider_OnValueChange(float value)
        {
            int spriteIndex = Mathf.CeilToInt(value);

            physicalObject.SpriteIndex = spriteIndex;
            physicalObject.RefreshSerializableData();
            RefreshObjectSprite();
        }


        private void OnHeightChanged(float additionalHeight)
        {
            mainLayoutElement.preferredHeight = InitialHeight + additionalHeight;
        }

        #endregion
    }
}
