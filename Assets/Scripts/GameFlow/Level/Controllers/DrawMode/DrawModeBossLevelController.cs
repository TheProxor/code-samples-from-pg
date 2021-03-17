using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class DrawModeBossLevelController : DrawModeLevelController
    {
        #region Ctor

        public DrawModeBossLevelController(ILevelEnvironment _levelEnvironment)
            : base(_levelEnvironment) { }

        #endregion



        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = base.IsControllerEnabled;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;

                //TODO solve double deinitialize invocation issue, maxim.ak
                if (context != null)
                {
                    result &= !context.IsBonusLevel;
                    result &= context.IsBossLevel;
                }

                return result;
            }
        }


        protected override void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 position)
        {
            InvokeLevelStateChangedEvent(LevelState.ReturnToInitial);

            foreach (var levelObject in Level.levelObjects)
            {
                if (!levelObject.IsHardReturnToInitialState)
                {
                    levelObject.ReturnToInitialState();
                }
            }

            GameServices.Instance.LevelControllerService.ReturnToInitialState();

            InvokeLevelStateChangedEvent(LevelState.Playing);
        }

        #endregion
    }
}

