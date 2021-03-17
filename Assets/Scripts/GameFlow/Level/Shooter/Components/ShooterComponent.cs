namespace Drawmasters.Levels
{
    public abstract class ShooterComponent
    {
        #region Fields

        protected Shooter shooter;

        #endregion


        #region Methods

        public virtual void Initialize(Shooter _shooter)
        {
            shooter = _shooter;
        }

        public abstract void Deinitialize();

        public abstract void StartGame();

        #endregion
    }
}
