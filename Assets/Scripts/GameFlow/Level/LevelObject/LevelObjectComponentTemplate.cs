namespace Drawmasters.Levels
{
    public abstract class LevelObjectComponentTemplate<T> : LevelObjectComponent where T: LevelObject, new()
    {
        #region Fields

        protected T levelObject;

        #endregion



        #region Methods

        public virtual void Initialize(T _levelObject)
        {
            levelObject = _levelObject;
        }

        #endregion
    }
}
