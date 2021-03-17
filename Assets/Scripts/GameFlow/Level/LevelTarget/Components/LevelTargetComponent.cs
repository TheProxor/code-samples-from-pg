namespace Drawmasters.Levels
{
    public abstract class LevelTargetComponent : LevelObjectComponent
    {
        #region Fields

        protected LevelTarget levelTarget;

        #endregion



        #region Methods

        public virtual void Initialize(LevelTarget _levelTarget)
        {
            levelTarget = _levelTarget;
        }

        #endregion
    }
}
