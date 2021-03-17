using UnityEngine;
using System.Collections.Generic;
using Modules.General;


namespace Drawmasters.Levels
{
    public class LevelTargetSlowSpikesComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] private CollisionNotifier hitLevelTargetCollisionNotifier = default;

        // callbacks on disable dont work correct on unity 2018.x
        // https://issuetracker.unity3d.com/issues/ontriggerexit2d-is-not-happening-when-other-colliding-object-is-beeing-destroyed?_ga=2.255655496.1114288414.1574659337-1498481534.1569474615
        // https://forum.unity.com/threads/physics2dsettings-callbacks-on-disable-doesnt-work-in-2018-2-3f1.545618/
        private List<Rigidbody2D> slowedRigidbody;
        private Dictionary<Rigidbody2D, SchedulerTask> slowTasks;
        private List<FixedJoint2D> fixedJoint2Ds;

        #endregion



        #region Methods

        public override void Enable()
        {
            slowedRigidbody = new List<Rigidbody2D>();
            slowTasks = new Dictionary<Rigidbody2D, SchedulerTask>();
            fixedJoint2Ds = new List<FixedJoint2D>();

            hitLevelTargetCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            hitLevelTargetCollisionNotifier.OnCustomTriggerExit2D += HitLevelTargetCollisionNotifier_OnCustomTriggerExit2D;
        }


        public override void Disable()
        {
            hitLevelTargetCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            hitLevelTargetCollisionNotifier.OnCustomTriggerExit2D -= HitLevelTargetCollisionNotifier_OnCustomTriggerExit2D;

            foreach (var pair in slowTasks)
            {
                SchedulerTask task = pair.Value;
                Scheduler.Instance.UnscheduleTask(task);
            }

            foreach (var i in fixedJoint2Ds)
            {
                Object.Destroy(i);
            }

            fixedJoint2Ds.Clear();
            slowedRigidbody.Clear();
            slowTasks.Clear();
        }


        private LevelTarget FindCollidedLevelTarget(Collider2D collision)
        {
            LevelTarget result = default;

            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return result;
            }

            if (collidableObject.Type == CollidableObjectType.EnemyTrigger)
            {
                result = collidableObject.LevelTarget;
            }

            return result;
        }


        private Rigidbody2D FindCollidedLevelTargetRigidbody(Collider2D collision)
        {
            Rigidbody2D result = default;

            LevelTarget levelTarget = FindCollidedLevelTarget(collision);
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (levelTarget != null &&
                collidableObject != null)
            {
                LevelTargetLimbPart levelTargetLimbPart = collidableObject.GetComponent<LevelTargetLimbPart>();

                if (levelTarget == null)
                {
                    CustomDebug.Log("Wrong object reference on enemy bone");
                    return result;
                }

                result = levelTarget.Ragdoll2D.GetRigidbody(levelTargetLimbPart.BoneName);
            }

            return result;
        }


        private void SlowRigidbdoy(Rigidbody2D rb)
        {
            SchedulerTask schedulerTask = Scheduler.Instance.CallMethodWithDelay(rb, () =>
              {
                  if (rb != null)
                  {
                      rb.velocity = Vector2.zero;
                      rb.angularVelocity = 0.0f;

                      var joint = sourceLevelObject.gameObject.AddComponent<FixedJoint2D>();

                      joint.connectedBody = rb;
                      fixedJoint2Ds.Add(joint);
                  }
              }, IngameData.Settings.commonSpikesSettings.delayForLevelTargetFix);

            slowTasks.Add(rb, schedulerTask);
        }


        private void FreeRigidbody(Rigidbody2D rb)
        {
            if (!slowedRigidbody.Contains(rb))
            {
                return;
            }

            if (slowTasks.TryGetValue(rb, out SchedulerTask schedulerTask))
            {
                Scheduler.Instance.UnscheduleTask(schedulerTask);
                slowTasks.Remove(rb);
            }

            FixedJoint2D joint = fixedJoint2Ds.Find(e => e.connectedBody == rb);
            Object.Destroy(joint);
            fixedJoint2Ds.Remove(joint);

            slowedRigidbody.Remove(rb);
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D collision)
        {
            Rigidbody2D foundRigidbody = FindCollidedLevelTargetRigidbody(collision);

            LevelTarget levelTarget = FindCollidedLevelTarget(collision);

            if (foundRigidbody != null &&
                !slowedRigidbody.Contains(foundRigidbody) &&
                levelTarget != null)
            {
                bool shouldSlowDownRigidbody = IngameData.Settings.commonSpikesSettings.AllowSlowLimb(foundRigidbody.name) ||
                                               levelTarget.IsChoppedOffLimb(foundRigidbody.name);

                if (shouldSlowDownRigidbody)
                {
                    SlowRigidbdoy(foundRigidbody);
                    slowedRigidbody.Add(foundRigidbody);
                }
            }
        }


        private void HitLevelTargetCollisionNotifier_OnCustomTriggerExit2D(GameObject go, Collider2D collision)
        {
            Rigidbody2D foundRigidbody = FindCollidedLevelTargetRigidbody(collision);

            if (foundRigidbody != null &&
                slowedRigidbody.Contains(foundRigidbody))
            {
                FreeRigidbody(foundRigidbody);
            }
        }

        #endregion
    }
}
