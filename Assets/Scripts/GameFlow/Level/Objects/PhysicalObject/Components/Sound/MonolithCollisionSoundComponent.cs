using System;
using Drawmasters.ServiceUtil;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class MonolithCollisionSoundComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private const float velocityMagnitudeToStopSound = 2.0f;

        [SerializeField]
        [Enum(typeof(AudioKeys.Ingame))]
        private string soundKey = default;

        private SoundManager soundManager;

        private Guid soundGuid;
        private bool isObjectOnMonolith;

        #endregion



        #region Methods

        public override void Enable()
        {
            soundManager = SoundManager.Instance;

            bool isEnabled = !GameServices.Instance.LevelEnvironment.Context.IsBonusLevel;

            if (!isEnabled)
            {
                return;
            }

            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
            collisionNotifier.OnCustomCollisionExit2D += CollisionNotifier_OnCustomCollisionExit2D;

            MonoBehaviourLifecycle.OnFixedUpdate += MonoBehaviourLifecycle_OnFixedUpdate;
        }


        public override void Disable()
        {
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
            collisionNotifier.OnCustomCollisionExit2D -= CollisionNotifier_OnCustomCollisionExit2D;

            MonoBehaviourLifecycle.OnFixedUpdate -= MonoBehaviourLifecycle_OnFixedUpdate;

            soundManager.StopSound(soundGuid);
        }


        private bool IsMonolith(Collision2D collision)
        {
            bool result = default;

            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return false;
            }

            result |= (collidableObject.Type == CollidableObjectType.Monolith);

            return result;
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject refObject, Collision2D collision)
        {
            if (IsMonolith(collision))
            {
                isObjectOnMonolith = true;
            }
        }


        private void CollisionNotifier_OnCustomCollisionExit2D(GameObject go, Collision2D collision)
        {
            if (IsMonolith(collision))
            {
                isObjectOnMonolith = false;
                soundManager.StopSound(soundGuid);
            }
        }


        private void MonoBehaviourLifecycle_OnFixedUpdate(float fixedDeltaTime)
        {
            if (isObjectOnMonolith)
            {
                if (!soundManager.IsActive(soundGuid) &&
                   (sourceLevelObject.Rigidbody2D.velocity.magnitude > velocityMagnitudeToStopSound))
                {
                    soundGuid = soundManager.PlaySound(soundKey, isLooping : true);
                }
                else if (soundManager.IsActive(soundGuid) &&
                        (sourceLevelObject.Rigidbody2D.velocity.magnitude < velocityMagnitudeToStopSound))
                {
                    soundManager.StopSound(soundGuid);
                }
            }
        }

        #endregion
    }
}
