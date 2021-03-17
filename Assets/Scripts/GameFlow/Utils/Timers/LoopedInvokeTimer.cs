using System;


namespace Drawmasters.Utils
{
    public class LoopedInvokeTimer
    {
        #region Fields
        
        private readonly Action onLoopUpdated;
        private readonly float updateRate;
        private float currentTime;

        #endregion



        #region Properties

        public bool IsTimerActive { get; protected set; }

        #endregion



        #region Ctor
        
        public LoopedInvokeTimer(Action _onLoopUpdated,
            float _updateRate = 1.0f)
        {
            onLoopUpdated = _onLoopUpdated;
            updateRate = _updateRate;
        }

        #endregion



        #region Public methods
        
        public void Start()
        {
            if (IsTimerActive)
            {
                CustomDebug.Log("Timer already started.");
            }
            else
            {
                MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            }
            
            IsTimerActive = true;
        }


        public void Stop()
        {
            if (IsTimerActive)
            {
                MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            }
            else
            {
                CustomDebug.Log("Timer wasn't started yet.");
            }

            IsTimerActive = false;
        }


        public void Reset() =>
            currentTime = 0.0f;

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            currentTime += deltaTime;

            if (currentTime > updateRate)
            {
                onLoopUpdated?.Invoke();
                
                currentTime = 0.0f;
            }
        }

        #endregion
    }
}