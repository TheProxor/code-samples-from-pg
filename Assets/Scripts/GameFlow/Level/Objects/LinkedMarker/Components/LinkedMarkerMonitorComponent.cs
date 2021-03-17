using System;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public abstract class LinkedMarkerMonitorComponent : LevelObjectComponentTemplate<LinkedMarkerLevelObject>
    {
        #region Fields

        public static event Action<LinkedMarkerLevelObject, LevelObject> OnShouldHide;

        protected readonly LinkedMarkerLevelObjectSettings settings;
        protected readonly List<LevelObject> monitoredObjects;

        #endregion



        #region Ctor

        public LinkedMarkerMonitorComponent()
        {
            settings = IngameData.Settings.linkedMarkerLevelObjectSettings;
            monitoredObjects = new List<LevelObject>();
        }

        #endregion



        #region Methods

        public override void Initialize(LinkedMarkerLevelObject _levelObject)
        {
            base.Initialize(_levelObject);

            LinkedMarkerShowHideComponent.OnShouldStartMonitor += LinkedMarkerShowHideComponent_OnShouldStartMonitor;
        }


        public override void Enable()
        {
        }


        public override void Disable()
        {
            LinkedMarkerShowHideComponent.OnShouldStartMonitor -= LinkedMarkerShowHideComponent_OnShouldStartMonitor;

            StopMonitor();
            monitoredObjects.Clear();
        }


        protected void InvokeOnShouldHide(LevelObject monitoredObject)
        {
            StopMonitor();

            OnShouldHide?.Invoke(levelObject, monitoredObject);
        }

        protected virtual void StartMonitor()
        {
            if (monitoredObjects != null)
            {
                foreach (var monitoredObject in monitoredObjects)
                {
                    monitoredObject.OnGameFinished += MonitoredObject_OnGameFinished;
                }
            }
        }


        protected virtual void StopMonitor()
        {
            foreach (var monitoredObject in monitoredObjects)
            {
                if (monitoredObject == null)
                {
                    CustomDebug.Log($"monitoredObject in null in {this}");
                }
                else
                {
                    monitoredObject.OnGameFinished -= MonitoredObject_OnGameFinished;
                }
            }
        }


        protected abstract List<LevelObject> SelectObjectsToMonitor(List<LevelObject> allLink);


        #endregion



        #region Events handlers

        private void LinkedMarkerShowHideComponent_OnShouldStartMonitor(LinkedMarkerLevelObject anotherLevelObject, List<LevelObject> _links)
        {
            if (levelObject != anotherLevelObject)
            {
                return;
            }

            monitoredObjects.Clear();
            monitoredObjects.AddRange(SelectObjectsToMonitor(_links));

            StartMonitor();
        }


        private void MonitoredObject_OnGameFinished(LevelObject levelObject) =>
            InvokeOnShouldHide(levelObject);

        #endregion
    }
}
