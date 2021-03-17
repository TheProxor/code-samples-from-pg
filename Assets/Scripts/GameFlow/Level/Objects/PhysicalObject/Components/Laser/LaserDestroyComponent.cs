using UnityEngine;
using DG.Tweening;
using System;
using Modules.Sound;
using Drawmasters.Effects;
using Modules.General;

namespace Drawmasters.Levels
{
    public class LaserDestroyComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        public static Action<PhysicalLevelObject> OnStartDestroy;

        public static event Action<bool> OnShouldSetLaserSoundEnabled;

        private SpriteRenderer mainSpriteRenderer = default;

        private string effectKey;
        private string endCorroseEffectKey;

        private EffectHandler destroyEffectHandler;

        private bool wasDestroyStarted;

        #endregion



        #region Methods

        public override void Enable()
        {
            sourceLevelObject.OnShouldStartLaserDestroy += StartDestroy;
            ConnectedObjectComponent.OnShouldLaserDestroy += ConnectedObjectComponent_OnShouldCorrose;

            mainSpriteRenderer = sourceLevelObject.SpriteRenderer;

            if (sourceLevelObject is Spikes spike && spike.Color.HasValue)
            {
                mainSpriteRenderer.color = spike.Color.Value;
            }
            else
            {
                mainSpriteRenderer.color = Color.white;
            }

            mainSpriteRenderer.enabled = true;
            wasDestroyStarted = false;

            effectKey = IngameData.Settings.laserSettings.GetDestroyEffect(sourceLevelObject.PhysicalData);
            endCorroseEffectKey = IngameData.Settings.laserSettings.GetDestroyEndEffect(sourceLevelObject.PhysicalData);
        }


        public override void Disable()
        {
            sourceLevelObject.OnShouldStartLaserDestroy -= StartDestroy;
            ConnectedObjectComponent.OnShouldLaserDestroy -= ConnectedObjectComponent_OnShouldCorrose;

            if (destroyEffectHandler != null && !destroyEffectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(destroyEffectHandler);
            }

            destroyEffectHandler = null;

            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        private void StartDestroy()
        {
            if (!wasDestroyStarted)
            {
                float volume = IngameData.Settings.physicalObject.CalculateVolume(sourceLevelObject.PhysicalData);
                float corrosiveTime = IngameData.Settings.physicalObject.FindLaserCorrosiveMultiplier(sourceLevelObject.PhysicalData) * volume;
                float timeToDisableVisual = corrosiveTime * IngameData.Settings.physicalObject.laserFactorToDisableObjectVisual;

                Color endCorroseColor = IngameData.Settings.laserSettings.laserHittedEndColor;

                mainSpriteRenderer
                    .DOColor(endCorroseColor, timeToDisableVisual)
                    .OnComplete(() => mainSpriteRenderer.enabled = false)
                    .SetId(this);

                sourceLevelObject.OnPreDestroy += SourceLevelObject_OnPreDestroy;

                if (sourceLevelObject is Spikes spikes)
                {
                    spikes.PlayFx(effectKey);
                }
                else
                {
                    destroyEffectHandler = EffectManager.Instance.CreateSystem(effectKey,
                                                                               true,
                                                                               sourceLevelObject.transform.position,
                                                                               sourceLevelObject.transform.rotation,
                                                                               sourceLevelObject.transform,
                                                                               shouldOverrideLoops: false);
                    if (destroyEffectHandler != null)
                    {
                        destroyEffectHandler.Play();
                    }
                }

                wasDestroyStarted = true;

                OnShouldSetLaserSoundEnabled?.Invoke(true);

                Scheduler.Instance.CallMethodWithDelay(this, sourceLevelObject.DestroyObject, corrosiveTime);

                OnStartDestroy?.Invoke(sourceLevelObject);
            }
        }

        #endregion



        #region Events handlers

        private void SourceLevelObject_OnPreDestroy(PhysicalLevelObject obj)
        {
            if (sourceLevelObject != obj)
            {
                return;
            }

            if (sourceLevelObject is Spikes spikes)
            {
                const float TileWidth = 13.4f;
                int tilesCount = Mathf.RoundToInt(sourceLevelObject.SpriteRenderer.size.x / TileWidth);

                SpriteRenderer spriteRendererDecal = spikes.SpriteRenderer;

                for (int i = 0; i < tilesCount; i++)
                {
                    Vector3 fxPos = spriteRendererDecal.transform.position;
                    fxPos = fxPos.SetX(spriteRendererDecal.transform.localPosition.x + ((-sourceLevelObject.SpriteRenderer.size.x + TileWidth) * 0.5f + i * TileWidth));

                    EffectManager.Instance.PlaySystemOnce(endCorroseEffectKey,
                                                          fxPos,
                                                          sourceLevelObject.transform.rotation);
                }
            }
            else
            {
                EffectManager.Instance.PlaySystemOnce(endCorroseEffectKey,
                                                      sourceLevelObject.transform.position,
                                                      sourceLevelObject.transform.rotation);

                SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.ACIDCUBESPHEREDESTRUCT);
            }

            OnShouldSetLaserSoundEnabled?.Invoke(false);

            sourceLevelObject.OnPreDestroy -= SourceLevelObject_OnPreDestroy;
        }


        private void ConnectedObjectComponent_OnShouldCorrose(PhysicalLevelObject obj)
        {
            if (obj == sourceLevelObject)
            {
                StartDestroy();
            }
        }

        #endregion
    }
}
