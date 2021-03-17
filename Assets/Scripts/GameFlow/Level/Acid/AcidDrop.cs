using UnityEngine;
using System;
using Drawmasters.Effects;
using Modules.General;

namespace Drawmasters.Levels
{
    public class AcidDrop : MonoBehaviour
    {
        #region Fields

        public event Action<AcidDrop> OnShouldDestroy;

        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private Rigidbody2D mainRigidbody2D = default;

        private float destroyDelayOnMonolith;
        private SchedulerTask destroyTask;

        private EffectHandler effectHandler;

        #endregion



        #region Methods

        public void Initialize(float _destroyDelayOnMonolith, string effectKey)
        {
            destroyDelayOnMonolith = _destroyDelayOnMonolith;

            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
            collisionNotifier.OnCustomTriggerExit2D += CollisionNotifier_OnCustomTriggerExit2D;

            effectHandler = EffectManager.Instance.CreateSystem(effectKey,
                                                                true,
                                                                Vector3.zero,
                                                                Quaternion.identity,
                                                                transform,
                                                                TransformMode.Local);

            if (effectHandler != null)
            {
                effectHandler.transform.localScale = Vector3.one;
            }
        }


        public void Deinitialize()
        {
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
            collisionNotifier.OnCustomTriggerExit2D -= CollisionNotifier_OnCustomTriggerExit2D;

            if (Scheduler.Instance != null)
            {
                ResetDestoryTask();
            }

            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }
            effectHandler = null;

            OnShouldDestroy = null;
        }


        public void AddForce(Vector2 value)
        {
            mainRigidbody2D.AddForce(value, ForceMode2D.Impulse);
        }


        private void ResetDestoryTask()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            destroyTask = null;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject reference, Collision2D collision2D)
        {
            CollidableObject collidableObject = collision2D.gameObject.GetComponent<CollidableObject>();

            if (destroyTask == null &&
                collidableObject != null &&
                collidableObject.Type == CollidableObjectType.Monolith)
            {
                destroyTask = Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    OnShouldDestroy?.Invoke(this);
                    ResetDestoryTask();
                }, destroyDelayOnMonolith);
            }
        }


        private void CollisionNotifier_OnCustomTriggerExit2D(GameObject reference, Collider2D collision2D)
        {
            CollidableObject collidableObject = collision2D.gameObject.GetComponent<CollidableObject>();

            if (collidableObject != null &&
                collidableObject.Type == CollidableObjectType.Monolith)
            {
                ResetDestoryTask();
            }
        }

        #endregion
    }
}
