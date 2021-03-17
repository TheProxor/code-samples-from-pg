namespace Drawmasters.Levels
{
    public class ShooterImmortalityOnWinComponent : ShooterComponent
    {
        #region Methods

        public override void StartGame()
        {
            shooter.SetImmortal(false);

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        public override void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
        }

        #endregion



        #region Methods

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.AllTargetsHitted)
            {
                shooter.SetImmortal(true);
            }
        }

        #endregion
    }
}
