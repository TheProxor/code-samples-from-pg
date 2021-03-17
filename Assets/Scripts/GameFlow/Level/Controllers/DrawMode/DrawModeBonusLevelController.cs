using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class DrawModeBonusLevelController : DrawModeLevelController
    {
        #region Ctor

        public DrawModeBonusLevelController(ILevelEnvironment _levelEnvironment) : 
            base(_levelEnvironment)
        { }

        #endregion



        #region Abstract implementation

        protected override bool IsControllerEnabled
        {
            get
            {
                bool result = base.IsControllerEnabled;

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;

                if (context != null)
                {
                    result &= context.IsBonusLevel;
                    result &= !context.IsBossLevel;
                }

                return result;
            }
        }

        protected override void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 position)
        {
            GameServices.Instance.LevelControllerService.Projectile.ReturnToInitialState();

            InvokeLevelStateChangedEvent(LevelState.Playing);
        }

        #endregion
    }
}