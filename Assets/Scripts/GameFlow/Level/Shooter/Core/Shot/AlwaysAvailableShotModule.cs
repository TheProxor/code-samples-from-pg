using System;


namespace Drawmasters.Levels
{
    public class AlwaysAvailableShotModule : IShotModule
    {
        #region Ctor

        public AlwaysAvailableShotModule()
        {
        }

        #endregion



        #region IShotModule

        public event Action OnShotReady;


        public void Initialize()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }

        #endregion



        #region Event handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime) =>
            OnShotReady?.Invoke();

        #endregion
    }
}
