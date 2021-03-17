namespace Drawmasters.Levels
{
    public abstract class ForcemeterComponent : LevelObjectComponent
    {
        #region Fields

        protected ForcemeterLevelObject forcemeter;

        #endregion



        #region Methods

        public virtual void Initialize(ForcemeterLevelObject _forcemeter)
        {
            forcemeter = _forcemeter;
        }

        #endregion
    }
}