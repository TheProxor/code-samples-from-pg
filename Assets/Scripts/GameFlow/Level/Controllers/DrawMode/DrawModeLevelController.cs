using System;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class DrawModeLevelController : SwitchableLevelController, ILevelStateChangeReporter
    {
        #region ILevelStateChangeReporter

        public event Action<LevelState> OnLevelStateChanged;

        #endregion



        #region Fields

        protected readonly ILevelEnvironment levelEnvironment;

        private Level level;

        #endregion



        #region Properties

        protected Level Level
        {
            get
            {
                if (level == null)
                {
                    if (LevelsManager.HasFoundInstance)
                    {
                        level = LevelsManager.Instance.Level;
                    }
                    else
                    {
                        //hotfix maxim.ak
                        level = GameObject.FindObjectOfType<Level>();
                    }
                }

                return level;
            }
        }

        #endregion



        #region Ctor

        public DrawModeLevelController(ILevelEnvironment _levelEnvironment)
        {
            levelEnvironment = _levelEnvironment;
        }

        #endregion




        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = default;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;
                if (context != null)
                {
                    result = context.Mode.IsDrawingMode();
                    result &= !context.IsSceneMode;
                }

                return result;
            }
        }


        #endregion



        #region Overrided methods

        public override void CustomInitialize()
        {
            ShootersInputLevelController.OnDrawFinish += ShootersInputLevelController_OnDrawFinish;
            ShootersInputLevelController.OnStartDraw += ShootersInputLevelController_OnStartDraw;            
        }


        public override void CustomDeinitialize()
        {
            ShootersInputLevelController.OnDrawFinish -= ShootersInputLevelController_OnDrawFinish;
            ShootersInputLevelController.OnStartDraw -= ShootersInputLevelController_OnStartDraw;            
        }

        #endregion



        #region Protected methods

        protected void InvokeLevelStateChangedEvent(LevelState state)
        {
            OnLevelStateChanged?.Invoke(state);
        }

        #endregion



        #region Events handlers

        private void ShootersInputLevelController_OnDrawFinish(Shooter shooter, bool succes, Vector2 position)
        {
            if (succes)
            {
                InvokeLevelStateChangedEvent(LevelState.FinishDrawing);
                InvokeLevelStateChangedEvent(LevelState.Playing);
            }
        }


        protected abstract void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 position);

        #endregion
    }
}