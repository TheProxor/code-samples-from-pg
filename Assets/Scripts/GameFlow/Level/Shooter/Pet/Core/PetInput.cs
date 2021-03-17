using System;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Pets
{
    public class PetInput
    {
        #region Fields

        public event Action OnResetDraw;
        public event Action<Vector3> OnStartDraw;
        public event Action<Vector2, float> OnDraw; // direction, deltaTime
        public event Action<bool, Vector3> OnDrawFinish;

        public static event Action OnDrawingCanceled;

        private readonly LevelStageController stageController;

        #endregion



        #region Properties

        public bool IsDrawActive { get; private set; }

        public bool WasTrajectoryLeftRect { get; private set; }

        #endregion



        #region Class lifecycle

        public PetInput()
        {
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

            OnStartDraw?.Invoke(position);
        }


        public void InvokeOnDraw(Vector2 direction, float deltaTime) =>
            OnDraw?.Invoke(direction, deltaTime);
        

        public void InvokeOnFinishDraw(bool succes, Vector3 position)
        {
            if (!succes)
            {
                IsDrawActive = false;
            }

            OnDrawFinish?.Invoke(succes, position);
        }

        #endregion



        #region Events handlers

        private void StageController_OnStartChangeStage()
        {
            InvokeOnFinishDraw(false, Vector3.zero);

            IsDrawActive = false;

            OnResetDraw?.Invoke();
        }

        #endregion
    }
}
