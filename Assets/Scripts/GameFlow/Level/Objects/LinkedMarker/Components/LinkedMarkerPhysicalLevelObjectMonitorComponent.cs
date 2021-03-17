using System.Collections.Generic;
using System.Linq;


namespace Drawmasters.Levels
{
    public class LinkedMarkerPhysicalLevelObjectMonitorComponent : LinkedMarkerMonitorComponent
    {
        #region Ctor

        public LinkedMarkerPhysicalLevelObjectMonitorComponent()
        {
        }

        #endregion



        #region Methods

        protected override void StartMonitor()
        {
            base.StartMonitor();

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        protected override void StopMonitor()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            base.StopMonitor();
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            foreach (var monitoredObject in monitoredObjects)
            {
                if (monitoredObject.Rigidbody2D.velocity.magnitude > settings.physicalObjectsVelocityThreshold)
                {
                    InvokeOnShouldHide(monitoredObject);
                }
            }
        }


        protected override List<LevelObject> SelectObjectsToMonitor(List<LevelObject> allLink) =>
            allLink.Where(e => e is PhysicalLevelObject).ToList();

        #endregion
    }
}
