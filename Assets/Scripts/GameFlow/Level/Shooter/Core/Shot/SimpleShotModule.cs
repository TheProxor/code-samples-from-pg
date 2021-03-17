using Drawmasters.ServiceUtil;
using System;

namespace Drawmasters.Levels
{
    public class SimpleShotModule : IShotModule
    {
        #region Fields

        private readonly ShootersHitmastersInputLevelController inputController;

        #endregion



        #region Ctor

        public SimpleShotModule()
        {
            inputController = GameServices.Instance.LevelControllerService.LineInput;
        }

        #endregion



        #region IShotModule

        public event Action OnShotReady;

        public void Initialize()
        {
            inputController.OnAimingEnded += InputController_OnAimingEnded;
        }


        public void Deinitialize()
        {
            inputController.OnAimingEnded -= InputController_OnAimingEnded;
        }

        #endregion



        #region Event handlers

        private void InputController_OnAimingEnded()
        {
            OnShotReady?.Invoke();
        }

        #endregion
    }
}
