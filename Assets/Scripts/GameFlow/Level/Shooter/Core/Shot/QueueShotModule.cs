using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class QueueShotModule : IShotModule
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
            shooterInput.OnDrawFinish += ShooterInput_OnDragFinish;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            shooterInput.OnDrawFinish -= ShooterInput_OnDragFinish;
        }

        #endregion



        #region Ctor

        public QueueShotModule(ShooterInput _input)
        {
            shooterInput = _input;
        }

        #endregion


        #region Private methods

        public void Shot()
        {
            OnShotReady?.Invoke();
        }

        #endregion



        #region Events handlers

        private void ShooterInput_OnDragFinish(bool isSuccess, Vector3 position)
        {
            if (isSuccess)
            {
                Shot();
            }
        }

        #endregion
    }
}
