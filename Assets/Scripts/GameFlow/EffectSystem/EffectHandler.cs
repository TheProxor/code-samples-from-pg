using System;
using System.Collections.Generic;
using Drawmasters.Pool.Interfaces;
using UnityEngine;


namespace Drawmasters.Effects
{
    public partial class EffectHandler : MonoBehaviour, IPoolCallback, IColor
    {
        #region Nested Types

        private class RendererInfo
        {
            public Renderer renderer = default;
            public int initialSortingOrder = default;
            public int initialSortingLayerID = default;
            public string initialSortingLayerName = default;


            public RendererInfo(Renderer initialRenderer)
            {
                renderer = initialRenderer;
                initialSortingOrder = renderer.sortingOrder;
                initialSortingLayerName = renderer.sortingLayerName;
            }
        }

        #endregion



        #region Fields

        public event Action<EffectHandler> OnPushed;

        [SerializeField] private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        [SerializeField] private List<TrailRenderer> trailsRenderers = new List<TrailRenderer>();

        private readonly List<RendererInfo> particlesRenderers = new List<RendererInfo>();

        #endregion



        #region Properties

        public List<ParticleSystem> ParticleSystems => particleSystems;


        public List<TrailRenderer> TrailsRenderers => trailsRenderers;


        public bool ShouldReturnToPoolOnFinish { get; set; }


        public bool IsPaused
        {
            get
            {
                bool result = true;

                VisitAllParticleSystems(currentSystem =>
                {
                    result &= currentSystem.isPaused;
                });

                return result;
            }
        }


        public bool IsPlaying
        {
            get
            {
                bool result = false;

                VisitAllParticleSystems(currentSystem =>
                {
                    result |= currentSystem.isPlaying;
                });

                return result;
            }
        }


        public bool IsStopped
        {
            get
            {
                bool result = true;

                VisitAllParticleSystems(currentSystem =>
                {
                    result &= currentSystem.isStopped;
                });

                return result;
            }
        }


        public bool IsAlive
        {
            get
            {
                bool result = false;

                VisitAllParticleSystems(currentSystem =>
                {
                    result |= currentSystem.IsAlive();
                });

                return result;
            }
        }


        private List<RendererInfo> ParticlesRenderers
        {
            get
            {
                if (particlesRenderers == null || particlesRenderers.Count == 0)
                {
                    FillCacheRenderer();
                }

                return particlesRenderers;
            }
        }

        #endregion



        #region IColor

        public Color Color
        {
            get
            {
                Color result = default;

                if (ParticleSystems != null &&
                    ParticleSystems.Count > 0)
                {
                    ParticleSystem system = ParticleSystems.FirstObject();

                    if (system != null)
                    {
                        result = system.main.startColor.color;
                    }
                }

                return result;
            }
            set
            {
                if (ParticleSystems != null)
                {
                    for (int i = 0; i < ParticleSystems.Count; i++)
                    {
                        ParticleSystem system = ParticleSystems[i];

                        if (system != null)
                        {
                            ParticleSystem.MainModule mainModule = system.main;

                            mainModule.startColor = value;
                        }
                    }
                }
            }
        }

        #endregion



        #region IPoolCallback

        public bool InPool { get; private set; }

        public void OnPop()
        {
            InPool = false;
        }

        public void OnPush()
        {
            for (int i = 0; i < particlesRenderers.Count; i++)
            {
                particlesRenderers[i].renderer.sortingOrder = particlesRenderers[i].initialSortingOrder;
                particlesRenderers[i].renderer.sortingLayerID = particlesRenderers[i].initialSortingLayerID;
                particlesRenderers[i].renderer.sortingLayerName = particlesRenderers[i].initialSortingLayerName;
            }

            Stop();
            Clear();

            InPool = true;

            OnPushed?.Invoke(this);
        }

        #endregion



        #region Methods

        public void Clear()
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Clear();
            });

            VisitAllTrailsRenderers(currentTrailRenderer =>
            {
                currentTrailRenderer.Clear();
            });
        }


        public void Play(bool isPlayWithChildrens = true, bool withClear = true)
        {
            if (withClear)
            {
                Clear();
            }

            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Play(isPlayWithChildrens);
            });

            VisitAllTrailsRenderers(currentTrailRenderer =>
            {
                CommonUtility.SetObjectActive(currentTrailRenderer.gameObject, true);
            });
        }



        public void Stop()
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Stop();
            });

            VisitAllTrailsRenderers(currentTrailRenderer =>
            {
                CommonUtility.SetObjectActive(currentTrailRenderer.gameObject, false);
            });
        }


        public void Pause()
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Pause();
            });
        }


        public void Simulate(float time)
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Simulate(time);
            });

            VisitAllTrailsRenderers(currentTrailRenderer =>
            {
                CommonUtility.SetObjectActive(currentTrailRenderer.gameObject, true);
            });
        }


        public void Simulate(float time, bool isWithChildrens)
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Simulate(time, isWithChildrens);
            });

            VisitAllTrailsRenderers(currentTrailRenderer =>
            {
                CommonUtility.SetObjectActive(currentTrailRenderer.gameObject, true);
            });
        }


        public void Simulate(float time, bool isWithChildrens, bool isNeedRestart)
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Simulate(time, isWithChildrens, isNeedRestart);
            });

            VisitAllTrailsRenderers(currentTrailRenderer =>
            {
                CommonUtility.SetObjectActive(currentTrailRenderer.gameObject, true);
            });
        }


        public void Simulate(float time, bool isWithChildrens, bool isNeedRestart, bool isFixedTimeStep)
        {
            VisitAllParticleSystems(currentSystem =>
            {
                currentSystem.Simulate(time, isWithChildrens, isNeedRestart, isFixedTimeStep);
            });

            VisitAllTrailsRenderers((renderer) =>
            {
                CommonUtility.SetObjectActive(renderer.gameObject, true);
            });
        }


        public void SetSortingOrder(int value)
        {
            List<RendererInfo> renderers = ParticlesRenderers;
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].renderer.sortingOrder = value;
            }
        }


        public void AddSortingOrder(int signedValue)
        {
            List<RendererInfo> renderers = ParticlesRenderers;
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].renderer.sortingOrder = renderers[i].renderer.sortingOrder + signedValue;
            }
        }


        public void RemoveSortingOrder(int signedValue)
        {
            List<RendererInfo> renderers = ParticlesRenderers;
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].renderer.sortingOrder = renderers[i].renderer.sortingOrder - signedValue;
            }
        }


        public void ChangeSortingLayer(string layerName)
        {
            List<RendererInfo> renderers = ParticlesRenderers;
            for (int i = 0; i < particlesRenderers.Count; i++)
            {
                renderers[i].renderer.sortingLayerName = layerName;
            }
        }


        public void SetLoop(bool loop)
        {
            VisitAllParticleSystems((renderer) =>
            {
                var main = renderer.main;
                main.loop = loop;
            });
        }


        private void VisitAllParticleSystems(Action<ParticleSystem> visitorCallback)
        {
            if (visitorCallback == null)
            {
                return;
            }

            for (int i = 0, n = particleSystems.Count; i < n; i++)
            {
                ParticleSystem currentSystem = particleSystems[i];

                if (currentSystem != null)
                {
                    visitorCallback(currentSystem);
                }
            }
        }


        private void VisitAllTrailsRenderers(Action<TrailRenderer> visitorCallback)
        {
            for (int i = 0, n = trailsRenderers.Count; i < n; i++)
            {
                TrailRenderer currentTrailRenderer = trailsRenderers[i];

                if (currentTrailRenderer != null && visitorCallback != null)
                {
                    visitorCallback(currentTrailRenderer);
                }
            }
        }


        private void FillCacheRenderer()
        {
            for (int i = 0, n = particleSystems.Count; i < n; i++)
            {
                particlesRenderers.Add(new RendererInfo(particleSystems[i].GetComponent<Renderer>()));
            }

            for (int i = 0, n = trailsRenderers.Count; i < n; i++)
            {
                particlesRenderers.Add(new RendererInfo(trailsRenderers[i] as Renderer));
            }
        }


        [Sirenix.OdinInspector.Button]
        public void AddFxsDelay(float value)
        {
            foreach (var system in particleSystems)
            {
                ParticleSystem.MainModule mainModule = system.main;
                var i = mainModule.startDelay;
                i.constant += value;
                mainModule.startDelay = i;
            }
        }

        #endregion
    }
}
