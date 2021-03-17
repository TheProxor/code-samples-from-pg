using UnityEngine;
using System.Collections.Generic;
using Drawmasters.Effects.Helpers.MonitorHelper.Enum;
using Drawmasters.Effects.Helpers.MonitorHelper.Implementation;
using Drawmasters.Effects.Helpers.MonitorHelper.Interfaces;
using Drawmasters.Pool;
using Drawmasters.Pool.Interfaces;
using Drawmasters.Effects.Interfaces;
using Drawmasters.Levels;


namespace Drawmasters.Effects
{
    public partial class EffectManager : SingletonMonoBehaviour<EffectManager>, IInitializable, IUpdatable, IMenuItemRefreshable
    {
        #region Fields

        [SerializeField] [ResourceLink] private List<AssetLink> systemHandlersAssetsLink = new List<AssetLink>();

        private readonly Dictionary<string, AssetLink> cachedSystemAssetsLink = new Dictionary<string, AssetLink>();

        private readonly List<KeyValuePair<int, ParticleSystem>> sortingList = new List<KeyValuePair<int, ParticleSystem>>();

        #endregion



        #region Properties

        public IPoolHelper<string, EffectHandler> PoolHelper { get; private set; }

        private IEffectAliveHandler AliveHandler { get; set; }
        
        public Dictionary<MonitorEffectType, IMonitorHelper> Monitors { get; protected set; }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            FillParticlesDictionary();

            PoolHelper = new CommonPoolHelper<string, EffectHandler>(cachedSystemAssetsLink);

            AliveHandler = new EffectAliveHandler(PoolHelper, new EffectAliveHandler.Data(5f));

            Monitors = new Dictionary<MonitorEffectType, IMonitorHelper>
            {
                { MonitorEffectType.LevelUnload, new LevelUnloadEffectMonitorHelper() }
            };
        }

        #endregion



        #region IUpdatable

        public void CustomUpdate(float deltaTime)
        {
            AliveHandler.CustomUpdate(deltaTime);
        }

        #endregion



        #region Methods

        public EffectHandler CreateSystem(string effectName,
                                          bool isLooped,
                                          Vector3 position = default,
                                          Quaternion rotation = default,
                                          Transform parent = null,
                                          TransformMode transformMode = TransformMode.World,
                                          bool shouldOverrideLoops = false,
                                          MonitorEffectType monitorEffectType = MonitorEffectType.None)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                return null;
            }

            EffectHandler newSystem = PoolHelper.PopObject(effectName);

            if (newSystem != null)
            {
                if (shouldOverrideLoops)
                {
                    newSystem.SetLoop(isLooped);
                }

                newSystem.transform.SetParent((parent != null) ? (parent) : transform);

                switch (transformMode)
                {
                    case TransformMode.Local:
                        {
                            newSystem.transform.localPosition = position;
                            newSystem.transform.localRotation = rotation;
                        }
                        break;

                    case TransformMode.World:
                        {
                            newSystem.transform.position = position;
                            newSystem.transform.rotation = rotation;
                        }
                        break;
                }
            }
            else
            {
                CustomDebug.Log($"No {nameof(EffectHandler)} on effect with name {effectName}");
            }

            bool isActualLooped = isLooped && shouldOverrideLoops;

            if (newSystem != null && !isActualLooped)
            {
                AliveHandler.BindComponent(newSystem);
            }

            if (Monitors.TryGetValue(monitorEffectType, out IMonitorHelper helper))
            {
                helper.BindComponent(newSystem);
            }

            return newSystem;
        }


        public EffectHandler PlaySystemOnce(string name,
                                           Vector3 position = default,
                                           Quaternion rotation = default,
                                           Transform parent = null,
                                           TransformMode transformMode = TransformMode.World,
                                           MonitorEffectType monitorEffectType = MonitorEffectType.LevelUnload )
        {
            EffectHandler newSystem = CreateSystem(name, 
                false, position, rotation, parent, transformMode, monitorEffectType: monitorEffectType);

            if (newSystem != null)
            {
                newSystem.transform.localScale = Vector3.one;
                newSystem.Play();
            }

            return newSystem;
        }


        public void PlaySystemOnce(string name,
                                    Vector3 position,
                                    Quaternion rotation,
                                    int sortingLayerID,
                                    int order,
                                    Transform parent = null,
                                    TransformMode transformMode = TransformMode.World)
        {
            EffectHandler newSystem = CreateSystem(name, false, position, rotation, parent, transformMode);

            if (newSystem != null)
            {
                SortSystem(newSystem, sortingLayerID, order);

                newSystem.transform.localScale = Vector3.one;
                newSystem.Play();
            }
        }


        public void SetLoop(ParticleSystem system, bool loop)
        {
            ParticleSystem[] systems = system.GetComponentsInChildren<ParticleSystem>();

            var sysMain = system.main;
            sysMain.loop = loop;

            for (int i = 0; i < systems.Length; i++)
            {
                var main = systems[i].main;
                main.loop = loop;
            }
        }


        public void SortSystem(ParticleSystem system, int sortingLayerID, int order)
        {
            SortSystem(system, order);
            SetLayer(system, sortingLayerID);
        }


        public void SortSystem(EffectHandler system, int sortingLayerID, int order)
        {
            SortSystem(system, order);
            SetLayer(system, sortingLayerID);
        }


        public void SetLayer(ParticleSystem system, int sortingLayerID)
        {
            if ((system != null) && SortingLayer.IsValid(sortingLayerID))
            {
                ParticleSystem[] systems = system.GetComponentsInChildren<ParticleSystem>();

                Renderer particleRenderer = system.GetComponent<Renderer>();

                if ((particleRenderer != null) && (particleRenderer.enabled))
                {
                    particleRenderer.sortingLayerID = sortingLayerID;
                }


                for (int i = 0; i < systems.Length; i++)
                {
                    ParticleSystem sortingSystem = systems[i];

                    particleRenderer = sortingSystem.GetComponent<Renderer>();

                    if ((particleRenderer != null) && (particleRenderer.enabled))
                    {
                        particleRenderer.sortingLayerID = sortingLayerID;
                    }
                }
            }
        }


        public void SetLayer(EffectHandler system, int sortingLayerID)
        {
            if ((system != null) && SortingLayer.IsValid(sortingLayerID))
            {
                ParticleSystem[] systems = system.ParticleSystems.ToArray();

                Renderer particleRenderer;

                for (int i = 0; i < systems.Length; i++)
                {
                    ParticleSystem sortingSystem = systems[i];

                    particleRenderer = sortingSystem.GetComponent<Renderer>();

                    if ((particleRenderer != null) && (particleRenderer.enabled))
                    {
                        particleRenderer.sortingLayerID = sortingLayerID;
                    }
                }
            }
        }


        public void SortSystem(ParticleSystem system, int order)
        {
            if (system != null)
            {
                ParticleSystem[] systems = system.GetComponentsInChildren<ParticleSystem>();

                sortingList.Clear();

                Renderer particleRenderer = system.GetComponent<Renderer>();

                if ((particleRenderer != null) && (particleRenderer.enabled))
                {
                    sortingList.Add(new KeyValuePair<int, ParticleSystem>(particleRenderer.sortingOrder, system));
                }

                for (int i = 0; i < systems.Length; i++)
                {
                    ParticleSystem sortingSystem = systems[i];

                    particleRenderer = sortingSystem.GetComponent<Renderer>();

                    if ((particleRenderer != null) && (particleRenderer.enabled))
                    {
                        sortingList.Add(new KeyValuePair<int, ParticleSystem>(particleRenderer.sortingOrder, sortingSystem));
                    }
                }

                sortingList.Sort(delegate (KeyValuePair<int, ParticleSystem> pair1, KeyValuePair<int, ParticleSystem> pair2)
                {
                    return pair1.Key.CompareTo(pair2.Key);
                });

                for (int i = 0, n = sortingList.Count; i < n; i++)
                {
                    ParticleSystem sortedSystem = sortingList[i].Value;

                    particleRenderer = sortedSystem.GetComponent<Renderer>();

                    if (particleRenderer != null)
                    {
                        particleRenderer.sortingOrder = order + i;
                    }
                }

                sortingList.Clear();
            }
        }


        public void SortSystem(EffectHandler system, int order)
        {
            if (system != null)
            {
                ParticleSystem[] systems = system.ParticleSystems.ToArray();

                sortingList.Clear();

                Renderer particleRenderer = null;

                for (int i = 0; i < systems.Length; i++)
                {
                    ParticleSystem sortingSystem = systems[i];

                    particleRenderer = sortingSystem.GetComponent<Renderer>();

                    if ((particleRenderer != null) && (particleRenderer.enabled))
                    {
                        sortingList.Add(new KeyValuePair<int, ParticleSystem>(particleRenderer.sortingOrder, sortingSystem));
                    }
                }

                sortingList.Sort(delegate (KeyValuePair<int, ParticleSystem> pair1, KeyValuePair<int, ParticleSystem> pair2)
                {
                    return pair1.Key.CompareTo(pair2.Key);
                });

                for (int i = 0, n = sortingList.Count; i < n; i++)
                {
                    ParticleSystem sortedSystem = sortingList[i].Value;

                    particleRenderer = sortedSystem.GetComponent<Renderer>();

                    if (particleRenderer != null)
                    {
                        particleRenderer.sortingOrder = order + i;
                    }
                }

                sortingList.Clear();
            }
        }


        public void ReturnHandlerToPool(EffectHandler handler)
        {
            if (handler != null && !handler.InPool)
            {
                PoolHelper.PushObject(handler);
            }
        }


        private void FillParticlesDictionary()
        {
            cachedSystemAssetsLink.Clear();

            for (int i = 0; i < systemHandlersAssetsLink.Count; i++)
            {
                AssetLink link = systemHandlersAssetsLink[i];

                if (link.IsAssetExists)
                {
                    if (!cachedSystemAssetsLink.ContainsKey(link.Name))
                    {
                        cachedSystemAssetsLink.Add(link.Name, link);
                    }
                }
                else
                {
                    CustomDebug.LogWarning("Check particles system " + link.Name);
                }
            }
        }

        #endregion



        #region IMenuItemRefreshable

        public void RefreshFromMenuItem()
        {
            #if UNITY_EDITOR

            RefreshEffects();

            #endif
        }

        #endregion
    }
}
