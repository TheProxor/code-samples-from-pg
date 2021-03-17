using System;


namespace Drawmasters.Utils
{
    public class Timer
    {
        #region Fields

        private float currentTime;

        #endregion



        #region Properties

        public float RoundedValue => (float)Math.Round(currentTime, 2);

        #endregion



        #region Methods

        public void Start()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public void Stop()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        public void Reset()
        {
            currentTime = 0.0f;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            currentTime += deltaTime;
        }

        #endregion
    }
}
