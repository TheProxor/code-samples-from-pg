using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterInput
    {
        #region Fields

        public event Action OnResetDraw;
        public event Action<Vector3> OnStartDraw;
        public event Action<Vector3, bool> OnDraw;
        public event Action<bool, Vector3> OnDrawFinish;
        public static event Action OnShouldForbidInput;

        public static event Action OnDrawingCanceled;

        private Rect shooterLeftRect;
        private bool isTrajectoryLeftRect;
        
        private readonly Camera gameCamera;
        private readonly LevelStageController stageController;

        #endregion



        #region Properties

        public bool IsDrawActive { get; private set; }

        public bool WasTrajectoryLeftRect { get; private set; }

        #endregion



        #region Class lifecycle

        public ShooterInput(Vector3 shooterPosition)
        {
            gameCamera = IngameCamera.Instance.Camera;

            shooterLeftRect = IngameData.Settings.shooter.collision.collisionRect;
            shooterLeftRect.position = shooterPosition
                .SetY(shooterPosition.y + IngameData.Settings.shooter.collision.collisionRectOffset.y)
                .SetX(shooterPosition.x + IngameData.Settings.shooter.collision.collisionRectOffset.x);

            stageController = GameServices.Instance.LevelControllerService.Stage;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            stageController.OnStartChangeStage += StageController_OnStartChangeStage;
        }


        public void Deinitialize()
        {
            stageController.OnStartChangeStage -= StageController_OnStartChangeStage;
        }


        public void InvokeOnStartDraw(Vector3 position)
        {
            IsDrawActive = true;
            isTrajectoryLeftRect = false;

            OnStartDraw?.Invoke(position);
        }


        public void InvokeOnDraw(Vector3 position)
        {
            if (!isTrajectoryLeftRect)
            {
                Vector3 touchWorldPosition = gameCamera.ScreenToWorldPoint(position);
                isTrajectoryLeftRect = !shooterLeftRect.Contains(touchWorldPosition);

                if (isTrajectoryLeftRect)
                {
                    Vector3 nearestPositionToRect = CommonUtility.NearestPointOnRect(shooterLeftRect, gameCamera.ScreenToWorldPoint(position));
                    Vector3 rectPosition = gameCamera.WorldToScreenPoint(nearestPositionToRect);
                    OnDraw?.Invoke(rectPosition, isTrajectoryLeftRect);
                }
            }
            
            OnDraw?.Invoke(position, isTrajectoryLeftRect);
        }


        public void InvokeOnFinishDraw(bool succes, Vector3 position)
        {
            if (!succes)
            {
                IsDrawActive = false;
            }

            WasTrajectoryLeftRect = isTrajectoryLeftRect;
            bool isFinishedSuccess = succes && isTrajectoryLeftRect;
            isTrajectoryLeftRect = false;
            
            OnDrawFinish?.Invoke(isFinishedSuccess, position);

            if (!isFinishedSuccess)
            {
                OnDrawingCanceled?.Invoke();
            }
        }


        public void ForbidInput() =>
            OnShouldForbidInput?.Invoke();

        #endregion



        #region Events handlers

        private void StageController_OnStartChangeStage()
        {
            InvokeOnFinishDraw(false, Vector3.zero);

            IsDrawActive = false;
            isTrajectoryLeftRect = false;

            OnResetDraw?.Invoke();
        }

        #endregion

    }
}
