namespace Drawmasters.Levels
{
    public class CommonLeftObjectHandler : ILeftObjectHandler
    {
        #region IInitializable

        public void Initialize()
        {
            
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            
        }

        #endregion



        #region ILeftObjectHandler

        public void HandleKilledTarget(LevelTarget killedTarget)
        {
            
        }


        public void HandleLeftTarget(LevelTarget leftTarget)
        {
            leftTarget.MarkHitted();
        }

        #endregion
    }
}

