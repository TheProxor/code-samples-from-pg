namespace Drawmasters.Levels
{
    public abstract class SwitchableLevelController : ILevelController
    {
        #region Properties

        protected abstract bool IsControllerEnabled { get; }

        #endregion



        #region ILevelController

        public void Initialize()
        {
            if (IsControllerEnabled)
            {
                CustomInitialize();
            }
        }


        public void Deinitialize()
        {
            if (IsControllerEnabled)
            {
                CustomDeinitialize();
            }
        }

        #endregion



        #region Abstract methods

        public abstract void CustomInitialize();

        public abstract void CustomDeinitialize();

        #endregion
    }
}

