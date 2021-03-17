namespace Drawmasters.Levels
{
    public abstract class LiquidComponent : LevelObjectComponent
    {
        #region Fields

        protected LiquidLevelObject liquid;

        #endregion



        #region Methods

        public virtual void Initialize(LiquidLevelObject _liquid)
        {
            liquid = _liquid;
        }

        #endregion
    }
}
