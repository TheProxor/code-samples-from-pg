
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class SpikesLevelObjectExtension : PhysicalLevelObjectExtension
    {
        #region Fields

        private float TileStep = 13.4f;

        [SerializeField] private FloatInputUi widthChange = default;
        [SerializeField] private BoolInputUi partLinkSwitch = default;

        private EditorSpikes spikesObjects;

        #endregion



        #region Methods

        public override void Init(EditorLevelObject levelObject)
        {
            base.Init(levelObject);

            spikesObjects = levelObject as EditorSpikes;

            if (spikesObjects != null)
            {
                widthChange.Init("Tiles Count", (int)(spikesObjects.Width / TileStep), 1.0f);
                spikesObjects.RefreshSerializableData();

                partLinkSwitch.Init("Is part of link", spikesObjects.IsLinkedObjectsPart);
            }
        }


        protected override void SubscribeOnEvents()
        {
            base.SubscribeOnEvents();

            widthChange.OnValueChange += WidthChange_OnValueChange;
            partLinkSwitch.OnValueChange += PartLinkSwitch_OnValueChange;
        }


        protected override void UnsubscribeFromEvents()
        {
            widthChange.OnValueChange -= WidthChange_OnValueChange;
            partLinkSwitch.OnValueChange -= PartLinkSwitch_OnValueChange;

            base.UnsubscribeFromEvents();
        }

        #endregion



        #region Events handlers

        private void WidthChange_OnValueChange(float value)
        {
            int tiles = (int) value;
            float perfectWidth = tiles * EditorSpikes.TileStep;

            spikesObjects.Width = perfectWidth;
            spikesObjects.RefreshSerializableData();
        }


        private void PartLinkSwitch_OnValueChange(bool value)
        {
            spikesObjects.IsLinkedObjectsPart = value;
            spikesObjects.RefreshSerializableData();
        }

        #endregion
    }
}
