using Drawmasters.Lifecycle;
using UnityEngine;


namespace Drawmasters
{
    public class TrackableMonoBehaviour : MonoBehaviour, ITrackable
    {
        protected ILifecycleTracker tracker;
        protected string fullHierarchyName;
        protected bool wasTrackerCreated;
        
        public ILifecycleTracker Tracker
        {
            get
            {
                if (tracker == null && !wasTrackerCreated)
                {
                    tracker = new LifecycleTracker(gameObject.FullHierarchyName());
                    
                    wasTrackerCreated = true;
                }

                return tracker;
            }
        }


        public string FullHierarchyName
        {
            get
            {
                if (string.IsNullOrEmpty(fullHierarchyName))
                {
                    fullHierarchyName = gameObject.FullHierarchyName();
                }

                return fullHierarchyName;
            }
        }

        protected void InnerInitialize() => Tracker.Initialize();

        protected void InnerDeinitialize() => Tracker.Deinitialize();

        private void OnDestroy()
        {
            if (Tracker != null)
            {
                Tracker.OnDestroy();
            }
            else
            {
                #if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        throw new TrackingRequiredException(FullHierarchyName);
                    }
                #endif
            }
        }
    }
}