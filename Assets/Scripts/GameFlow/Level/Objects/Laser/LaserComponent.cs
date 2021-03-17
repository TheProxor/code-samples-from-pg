namespace Drawmasters.Levels
{
    public abstract class LaserComponent : LevelObjectComponent
    {
        #region Fields

        protected LaserLevelObject laser;

        #endregion



        #region Methods

        public virtual void Initialize(LaserLevelObject _laser)
        {
            laser = _laser;
        }

        #endregion
    }
}
