using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LeaveObjectsHelpers
    {
        #region Fields

        public event Action OnAllObjectsLeftZone;

        private readonly Dictionary<int, HashSet<PhysicalLevelObject>> allObjects =
            new Dictionary<int, HashSet<PhysicalLevelObject>>();
        
        private readonly HashSet<PhysicalLevelObject> enteredZoneObjects = new HashSet<PhysicalLevelObject>();
        
        private HashSet<PhysicalLevelObject> currentStageObjects = new HashSet<PhysicalLevelObject>();

        private readonly float upperMonolithCoordinate;
        private readonly BonusLevelController bonusLevelController;
        private LevelPhysicalObjectsController physicalObjectsController;

        #endregion
        
        
        
        #region Properties

        public int TrackedStageIndex { get; private set; } = -1;

        public bool IsTrackActive => TrackedStageIndex >= 0;
        
        #endregion



        #region Ctor

        public LeaveObjectsHelpers(LevelObjectMonolith _levelObjectMonolith, BonusLevelController _bonusLevelController)
        {
            upperMonolithCoordinate = FindUpperMonolithPoint(_levelObjectMonolith);
            bonusLevelController = _bonusLevelController;
        }

        #endregion



        #region Public methods

        public void Initialize()
        {
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            
            bonusLevelController.OnStageBegun += BonusLevelController_OnStageBegun;
            
            physicalObjectsController = GameServices.Instance.LevelControllerService.PhysicalObjects;
            List<PhysicalLevelObject> objectList = physicalObjectsController.AllObjects;
            
            foreach (var physicalLevelObject in objectList)
            {
                int key = physicalLevelObject.CurrentData.bonusData.stageIndex;

                if (allObjects.TryGetValue(key, out HashSet<PhysicalLevelObject> set))
                {
                    set.AddIfNotContains(physicalLevelObject);
                }
                else
                {
                    HashSet<PhysicalLevelObject> objectsSet = new HashSet<PhysicalLevelObject>();
                    objectsSet.Add(physicalLevelObject);
                    
                    allObjects.Add(key, objectsSet);
                }
            }
            
            enteredZoneObjects.Clear();
        }

        


        public void Deinitialize()
        {
            TrackedStageIndex = -1;
            
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            
            bonusLevelController.OnStageBegun -= BonusLevelController_OnStageBegun;
                       
            allObjects.Clear();
            enteredZoneObjects.Clear();
        }

        #endregion



        #region Private methods

        private void CheckEnterZone()
        {
            foreach (var physicalLevelObject in currentStageObjects)
            {
                if (physicalLevelObject == null)
                {
                    continue;
                }
                
                if (enteredZoneObjects.Contains(physicalLevelObject))
                {
                    continue;
                }

                if (IsObjectInZone(physicalLevelObject))
                {
                    enteredZoneObjects.Add(physicalLevelObject);
                }
            }
        }


        private void CheckLeftZone()
        {
            List<PhysicalLevelObject> buffer = new List<PhysicalLevelObject>(enteredZoneObjects.Count);
            
            foreach (var physicalLevelObject in enteredZoneObjects)
            {
                if (!IsObjectInZone(physicalLevelObject))
                {
                    buffer.Add(physicalLevelObject);
                }
            }

            foreach (var physicalLevelObject in buffer)
            {
                enteredZoneObjects.Remove(physicalLevelObject);
                currentStageObjects.Remove(physicalLevelObject);
            }
            
            buffer.Clear();
            currentStageObjects.RemoveWhere(i => i.IsNull());
            currentStageObjects.RemoveWhere(i => !physicalObjectsController.AllObjects.Contains(i));

            if (currentStageObjects.Count == 0)
            {
                OnAllObjectsLeftZone?.Invoke();

                TrackedStageIndex = -1;
            }
        }


        private bool IsObjectInZone(PhysicalLevelObject physicalLevelObject)
            => !physicalLevelObject.IsNull() && 
               physicalLevelObject.transform.position.y >= upperMonolithCoordinate;


        private static float FindUpperMonolithPoint(LevelObjectMonolith monolith)
        {
            float result = float.MinValue;

            for (int i = 0; i < monolith.Spline.GetPointCount(); i++)
            {
                Vector3 point = monolith.Spline.GetPosition(i);

                if (result < point.y)
                {
                    result = point.y;
                }
            }

            return result + monolith.transform.position.y;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (IsTrackActive)
            {
                CheckLeftZone();
                
                CheckEnterZone(); // this order execute to avoid marking left immediately after entering zone
            }
        }
        
        
        private void BonusLevelController_OnStageBegun(int stageIndex)
        {
            if (TrackedStageIndex != stageIndex)
            {
                TrackedStageIndex = stageIndex;

                if (!allObjects.TryGetValue(TrackedStageIndex, out currentStageObjects))
                {
                    CustomDebug.Log($"There are not any objects for stage: {stageIndex}");

                    currentStageObjects = new HashSet<PhysicalLevelObject>();
                }

                if (enteredZoneObjects.Count != 0)
                {
                    CustomDebug.Log($"Might be an error");
                }

                enteredZoneObjects.Clear();
            }
            else
            {
                CustomDebug.Log($"Index: {stageIndex} is already tracked");
            }
        }
        
        #endregion
    }
}

