using System.Collections.Generic;
using System.Linq;


namespace Drawmasters.Levels
{
    public class LinkedMarkerLevelTargetMonitorComponent : LinkedMarkerMonitorComponent
    {
        #region Fields

        private List<LevelTarget> savedMonitoredObjects;

        #endregion


        #region Ctor

        public LinkedMarkerLevelTargetMonitorComponent()
        {
        }

        #endregion



        #region Methods

        protected override void StartMonitor()
        {
            base.StartMonitor();

            foreach (var levelTarget in savedMonitoredObjects)
            {
                levelTarget.OnShouldApplyRagdoll += LevelTarget_OnShouldApplyRagdoll;
                levelTarget.OnHitted += LevelTarget_OnHitted;
            }
        }


        protected override void StopMonitor()
        {
            if (savedMonitoredObjects != null)
            {
                foreach (var levelTarget in savedMonitoredObjects)
                {
                    levelTarget.OnShouldApplyRagdoll -= LevelTarget_OnShouldApplyRagdoll;
                    levelTarget.OnHitted -= LevelTarget_OnHitted;
                }
            }

            base.StopMonitor();
        }

        #endregion



        #region Events handlers


        private void LevelTarget_OnHitted(LevelTarget levelTarget) =>
            InvokeOnShouldHide(levelTarget);


        private void LevelTarget_OnShouldApplyRagdoll(LevelTarget levelTarget) =>
            InvokeOnShouldHide(levelTarget);


        protected override List<LevelObject> SelectObjectsToMonitor(List<LevelObject> allLink)
        {
            savedMonitoredObjects = allLink.OfType<LevelTarget>().ToList();

            return allLink.Where(e => e is LevelTarget).ToList();
        }

        #endregion
    }
}
