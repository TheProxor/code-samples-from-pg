namespace Drawmasters.Levels
{
    public abstract class LiquidGraphicVisualComponent : LiquidComponent
    {
        #region Fields

        protected LiquidLevelObjectVisual visualSettings;

        #endregion



        #region Methods

        public override void Enable()
        {
            visualSettings = IngameData.Settings.liquidSettings.FindVisual(liquid.LoadedData.type);
        }

        #endregion
    }
}
