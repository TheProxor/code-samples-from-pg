using System;


namespace Drawmasters.Levels
{
    public class SingleShotModule : IShotModule
    {
        #region Fields

        private readonly ShooterInput shooterInput;

        #endregion



        #region IShotModule

        public event Action OnShotReady;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
        }

        #endregion



        #region Ctor

        public SingleShotModule(ShooterInput _input)
        {
            shooterInput = _input;
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.FinishDrawing)
            {
                if (shooterInput.IsDrawActive &&
                    shooterInput.WasTrajectoryLeftRect)
                {
                    OnShotReady?.Invoke();
                }
            }
        }

        #endregion
    }
}
