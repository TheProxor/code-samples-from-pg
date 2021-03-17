using UnityEngine;
using System;
using Modules.General;
using Modules.Sound;

namespace Drawmasters.Levels
{
    public abstract class ImpulseDestroyComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        public static event Action<string> OnShouldLog;

        protected const float minStrengthPercentDamageToLog = 0.5f;

        protected const float minImpulsForSound = 400.0f;
        protected const float soundCooldown = 0.05f;

        [SerializeField] private bool shouldEnableLogs = default;

        private static bool isSoundOnCooldown;
        private CommonPhysicalObjectsSettings commonSettings;

        #endregion



        #region Abtract methods

        protected abstract void HandleCollision(GameObject go, GameObject otherGameObject);

        #endregion



        #region Methods

        public override void Enable()
        {
            commonSettings = IngameData.Settings.physicalObject;

            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
            }
        }


        public override void Disable()
        {
            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;
            }

            isSoundOnCooldown = false;
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        protected void DestroyObject(PhysicalLevelObject physicalLevelObject, float receivedImpuls)
        {
            if (shouldEnableLogs)
            {
                CustomDebug.Log($"Impuls power is {receivedImpuls} created on object {physicalLevelObject.name}");
            }

            if (!isSoundOnCooldown &&
                receivedImpuls > minImpulsForSound)
            {
                string soundKey = commonSettings.GetImpulsSoundKey(physicalLevelObject.PhysicalData);
                float volume = commonSettings.GetImpulsSoundVolume(receivedImpuls, physicalLevelObject.PhysicalData);

                Guid soundGuid = SoundManager.Instance.PlaySound(soundKey);
                SoundManager.Instance.SetSoundVolume(soundGuid, volume);

                isSoundOnCooldown = true;
                Scheduler.Instance.CallMethodWithDelay(this, () => isSoundOnCooldown = false, soundCooldown);
            }

            if (receivedImpuls >= physicalLevelObject.Strength)
            {
                physicalLevelObject.DestroyObject();

                if (shouldEnableLogs)
                {
                    CustomDebug.Log($"Object {physicalLevelObject.name} destroyed object {physicalLevelObject.name}");
                }
            }
        }


        protected void InvokeLogEvent(string text)
        {
            OnShouldLog?.Invoke(text);
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject go, Collision2D collision)
        {
            HandleCollision(go, collision.gameObject);
        }

        #endregion
    }
}
