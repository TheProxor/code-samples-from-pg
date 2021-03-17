using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using System;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class ShootersHitmastersInputLevelController : SwitchableLevelController
    {
        #region Fields

        public event Action OnAimingEnded;

        private ShooterInput shooterInput;
        private Shooter shooter;

        #endregion



        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = false;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;

                if (context != null)
                {
                    result = context.Mode.IsHitmastersLiveOps();
                }

                return result;
            }
        }

        #endregion



        #region Overrided methods

        public override void CustomInitialize()
        {
            TouchManager.OnBegan += TouchManager_OnBegan;
            TouchManager.OnMove += TouchManager_OnMove;
            TouchManager.OnEnded += TouchManager_OnEnded;
        }

        

        public override void CustomDeinitialize()
        {
            TouchManager.OnBegan -= TouchManager_OnBegan;
            TouchManager.OnMove -= TouchManager_OnMove;
            TouchManager.OnEnded -= TouchManager_OnEnded;

            shooterInput.Deinitialize();
            shooterInput = null;
            shooter = null;
        }

        #endregion



        #region Public methods

        public void AddShooter(Shooter _shooter)
        {
            if (shooter != null)
            {
                return;
            }

            if (!IsControllerEnabled)
            {
                return;
            }

            shooter = _shooter;

            shooterInput = new ShooterInput(shooter.transform.position);
            shooterInput.Initialize();

            shooter.SetInputModule(shooterInput);
        }

        #endregion



        #region Events handlers

        private void TouchManager_OnEnded(bool isSuccess, Vector2 position)
        {
            shooterInput.InvokeOnFinishDraw(isSuccess, position);

            if (isSuccess)
            {
                OnAimingEnded?.Invoke();
            }
        }

        private void TouchManager_OnMove(Vector2 position)
        {
            shooterInput.InvokeOnDraw(position);

        }

        private void TouchManager_OnBegan(Vector2 position)
        {
            shooterInput.InvokeOnStartDraw(position);
        }

        #endregion
    }
}

