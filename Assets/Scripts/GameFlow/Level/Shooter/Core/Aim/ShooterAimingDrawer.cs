using System;
using Drawmasters.Effects;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ShooterAimingDrawer : IDeinitializable
    {
        #region Fields

        private EffectHandler effectHandler;
        private Guid drawSoundGuid;

        protected float clearDuration;

        protected const float initalClearDuration = 0.0f;

        #endregion



        #region Properties

        public abstract Vector2 CurrentDirection { get; protected set; }

        public abstract Vector2 StartDirection { get; }

        public abstract Vector2[] CurrentProjectileTrajectory { get; }

        protected abstract string UnderFingerFxKey { get; }

        protected abstract string DrawSfxKey { get; }

        public bool IsEnoughtTrajectoryDrawed
        {
            get
            {
                return CurrentProjectileTrajectory != null &&
                       CurrentProjectileTrajectory.Length > 1;

            }
        }
        
        public abstract float PathDistance { get; }

        #endregion



        #region Methods

        public virtual void Initialize(Transform levelTransform, WeaponType type)
        {
            effectHandler = EffectManager.Instance.CreateSystem(UnderFingerFxKey, true, default, default, levelTransform);

            ApplySettings(type);

            SetInitalClearDuration();
        }


        public virtual void Deinitialize()
        {
            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }

            effectHandler = null;

            StopSound();
            drawSoundGuid = Guid.Empty;

            SetInitalClearDuration();
        }


        public void SetClearDuration(float duration)
        {
            clearDuration = duration;
        }


        public void SetInitalClearDuration()
        {
            clearDuration = initalClearDuration;
        }


        public virtual void ClearDraw(bool isImmediately)
        {
            if (effectHandler != null)
            {
                effectHandler.Clear();
            }

            StopSound();
        }


        public virtual void DrawShotDirection(Vector2 startPosition, Vector2 touchPosition)
        {
            if (effectHandler != null)
            {
                effectHandler.transform.position = touchPosition;
            }
        }


        public virtual void EndDrawShotDirection(Vector2 startPosition, Vector2 touchPosition)
        {
            StopSound();
        }


        public virtual void StartDrawing(Vector2 touchPosition)
        {
            if (effectHandler != null)
            {
                effectHandler.Play();
                effectHandler.transform.position = touchPosition;
            }

            PlaySound();
        }


        protected abstract void ApplySettings(WeaponType type);


        protected void PlaySound()
        {
            if (!SoundManager.Instance.IsActive(drawSoundGuid))
            {
                drawSoundGuid = SoundManager.Instance.PlaySound(DrawSfxKey, isLooping: true);
            }
        }


        protected void StopSound()
        {
            SoundManager.Instance.StopSound(drawSoundGuid);
        }

        public virtual void SetReloadVisualEnabled(bool value)
        {
            if (effectHandler != null)
            {
                if (value)
                {
                    effectHandler.Play();
                }
                else
                {
                    effectHandler.Clear();
                    effectHandler.Pause();
                }
            }
        }

        #endregion
    }
}
