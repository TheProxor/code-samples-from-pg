using UnityEngine;


namespace Drawmasters.Helpers
{
    public class GameTimeScaleHelper : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly float targetTimeScale;

        private float previousTimeScale;

        #endregion



        #region Class lifecycle

        public GameTimeScaleHelper(float _targetTimeScale)
        {
            targetTimeScale = _targetTimeScale;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            previousTimeScale = Time.timeScale;

            TouchManager.OnBeganTouchAnywhere += TouchManager_OnBeganTouchAnywhere;
        }


        public void Deinitialize()
        {
            TouchManager.OnBeganTouchAnywhere -= TouchManager_OnBeganTouchAnywhere;

            if (Mathf.Approximately(previousTimeScale, 0.0f))
            {
                CustomDebug.Log($"Wrong lifecycle in {this}." +
                                $" Setting up 1.0f as default time scale");
                Time.timeScale = 1.0f;
            }
            else
            {
                Time.timeScale = previousTimeScale;
            }
        }

        #endregion



        #region Events handlers

        private void TouchManager_OnBeganTouchAnywhere()
        {
            TouchManager.OnBeganTouchAnywhere -= TouchManager_OnBeganTouchAnywhere;

            previousTimeScale = Time.timeScale;
            Time.timeScale = targetTimeScale;
        }

        #endregion
    }
}
