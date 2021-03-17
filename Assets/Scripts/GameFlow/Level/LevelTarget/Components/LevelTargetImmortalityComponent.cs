namespace Drawmasters.Levels
{
    public class LevelTargetImmortalityComponent : LevelTargetComponent
    {
        #region Methods

        public override void Enable()
        {
            levelTarget.OnShouldSetImmortal += LevelTarget_OnShouldSetImmortal;
        }


        public override void Disable()
        {
            levelTarget.OnShouldSetImmortal -= LevelTarget_OnShouldSetImmortal;
        }

        #endregion



        #region Events handlers

        private void LevelTarget_OnShouldSetImmortal(bool immortallityEnabled)
        {
            if (levelTarget.StandRigidbody != null)
            {
                levelTarget.StandRigidbody.simulated = !immortallityEnabled;
            }
        }

        #endregion
    }
}
