using System.Collections.Generic;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LiquidObjectsFullyCoveredComponent : LiquidComponent
    {
        #region Fields

        private const float CheckSecondsRate = 0.15f;

        // callbacks on disable dont work correct on unity 2018.x
        // https://issuetracker.unity3d.com/issues/ontriggerexit2d-is-not-happening-when-other-colliding-object-is-beeing-destroyed?_ga=2.255655496.1114288414.1574659337-1498481534.1569474615
        // https://forum.unity.com/threads/physics2dsettings-callbacks-on-disable-doesnt-work-in-2018-2-3f1.545618/
        private List<PhysicalLevelObject> objectsToCheck;
        private LiquidSettings settings;
        
        private float currentTime;

        #endregion



        #region Methods

        public override void Enable()
        {
            objectsToCheck = new List<PhysicalLevelObject>();

            liquid.CollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            settings = IngameData.Settings.liquidSettings;
            currentTime = 0f;
        }


        public override void Disable()
        {
            liquid.CollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;

            foreach (var checkObject in objectsToCheck)
            {
                Scheduler.Instance.UnscheduleAllMethodForTarget(checkObject);
            }
            
            objectsToCheck.Clear();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D collision)
        {
            // start check fully intersect here
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            PhysicalLevelObject obj = collidableObject.PhysicalLevelObject;

            if (obj != null &&
               !objectsToCheck.Contains(obj))
            {
                obj.OnPreDestroy += Obj_OnPreDestroy;
                objectsToCheck.Add(obj);
            }
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            currentTime += deltaTime;
            if (currentTime < CheckSecondsRate)
            {
                return;
            }
            currentTime = 0f;
            
            for (int i = objectsToCheck.Count - 1; i > -1; i--)
            {
                bool isSafeIndex = i < objectsToCheck.Count;
                if (!isSafeIndex)
                {
                    continue;
                }

                PhysicalLevelObject obj = objectsToCheck[i];

                Bounds colliderBounds = obj.MainCollider2D.bounds;

                Vector3[] points = {
                    colliderBounds.center + Vector3.up * colliderBounds.extents.y,
                    colliderBounds.center + Vector3.down * colliderBounds.extents.y,
                    colliderBounds.center + Vector3.right * colliderBounds.extents.x,
                    colliderBounds.center + Vector3.left * colliderBounds.extents.x,
                 };

                bool isFullyInside = true;

                foreach (var point in points)
                {
                    isFullyInside &= liquid.MainCollider2D.bounds.Contains(point);
                }

                if (isFullyInside)
                {
                    Scheduler.Instance.CallMethodWithDelay(obj, 
                        obj.DestroyObject, 
                        settings.fullyCoveredObjectsDestoyDelay);
                    
                    objectsToCheck.Remove(obj);
                }
            }
        }


        private void Obj_OnPreDestroy(PhysicalLevelObject obj)
        {
            obj.OnPreDestroy -= Obj_OnPreDestroy;

            if (objectsToCheck.Contains(obj))
            {
                Scheduler.Instance.UnscheduleAllMethodForTarget(obj);
                
                objectsToCheck.Remove(obj);
            }

        }

        #endregion
    }
}
