namespace Drawmasters.Levels
{
    public abstract class RopeComponent : LevelObjectComponent
    {
        #region Fields

        protected Rope rope;

        #endregion



        #region Methods

        public virtual void Initialize(Rope _rope)
        {
            rope = _rope;
        }

        #endregion
    }
}
