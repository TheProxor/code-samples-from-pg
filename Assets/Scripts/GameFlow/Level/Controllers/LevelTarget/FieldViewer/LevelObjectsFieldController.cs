using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class LevelObjectsFieldController : ILevelController
    {
        #region Fields

        private const float GameZoneSizeMultiplier = 1.2f;

        private Rect gameZoneRect;

        #endregion



        #region Methods

        public virtual void Initialize()
        {
            RecalculateGameZoneRect();
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            ScreenChangesMonitor.OnOrientationChange += ScreenChangesMonitor_OnOrientationChange;
        }


        public virtual void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            ScreenChangesMonitor.OnOrientationChange -= ScreenChangesMonitor_OnOrientationChange;
        }


        protected bool IsOutOfGameZone(Vector3 position) => !gameZoneRect.Contains(position);
        
        protected abstract void OnCheckGameZone();

        private void RecalculateGameZoneRect()
        {
            float multiplier = GameZoneSizeMultiplier;
            gameZoneRect = CommonUtility.CalculateGameZoneRect(IngameCamera.Instance.Camera, multiplier);
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime) =>
            OnCheckGameZone();


        private void ScreenChangesMonitor_OnOrientationChange(DeviceOrientation deviceOrientation) =>
            RecalculateGameZoneRect();

        #endregion
    }
}
