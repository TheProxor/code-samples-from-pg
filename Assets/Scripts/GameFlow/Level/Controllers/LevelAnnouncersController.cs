using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Pool;
using Drawmasters.Pool.Interfaces;

namespace Drawmasters.Levels
{
    public class LevelAnnouncersController : ILevelController
    {
        #region Nested types

        private class DelayData
        {
            public PerfectType perfectType = default;
            public float delay = default;
            public Vector3 position = default;
        }

        #endregion



        #region Fields

        private const float FirstAnnouncerDelay = 0.1f;

        private AnnouncerSettings settings;

        private ComponentPool poolForObject;

        private readonly Dictionary<LevelTarget, Queue<DelayData>> announcersQueue = 
            new Dictionary<LevelTarget, Queue<DelayData>>();

        private readonly List<Announcer> activeAnnouncers =
            new List<Announcer>();

        #endregion



        #region Properties

        public static bool IsEnabled
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.AbTest.UaAnnouncersEnabled, true);
            set => CustomPlayerPrefs.SetBool(PrefsKeys.AbTest.UaAnnouncersEnabled, value);
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            PerfectsManager.OnPerfectReceived += CurrencyManager_OnPerfectReceived;
#warning hotfix
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            settings = IngameData.Settings.announcerSettings;

            announcersQueue.Clear();

            if (poolForObject == null)
            {
                IPoolManager poolManager = PoolManager.Instance;

                poolForObject = poolManager.GetComponentPool(settings.announcerPrefab);
            }

        }


        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            PerfectsManager.OnPerfectReceived -= CurrencyManager_OnPerfectReceived;

            activeAnnouncers.RemoveAll(i => i.IsNull() || !i.IsActive);
            activeAnnouncers.ForEach(i =>  i.Deinitialize());
            activeAnnouncers.Clear();
        }


        private void PlayAnnouncer(PerfectType type, Vector3 position)
        {
            Sprite sprite = settings.GetSprite(type);
            if (sprite != null)
            {
                ShowAnnouncer(sprite, settings.lifeTime, position, settings.moveCurve, settings.alphaCurve, null);    
            }
        }


        private void ShowAnnouncer(Sprite sprite, float lifeTime, Vector3 position, AnimationCurve moveCurve, AnimationCurve alphaCurve, AnimationCurve scaleCurve)
        {
            if (!IsEnabled)
            {
                return;
            }

            MonoBehaviour poppedComponent = poolForObject.Pop();
            
            if (poppedComponent is Announcer announcer)
            {
                announcer.Init(poolForObject,
                               sprite,
                               position,
                               position.SetY(position.y + settings.offsetY),
                               lifeTime,
                               moveCurve,
                               alphaCurve,
                               scaleCurve);
                
                activeAnnouncers.Add(announcer);
            }
        }

        #endregion



        #region Events handlers

        private void CurrencyManager_OnPerfectReceived(PerfectType type, Vector3 position, LevelTarget levelTarget)
        {
            if (!announcersQueue.ContainsKey(levelTarget))
            {
                announcersQueue.Add(levelTarget, new Queue<DelayData>());
            }

            float delay = announcersQueue[levelTarget].Count == 0 ? FirstAnnouncerDelay : settings.announcersMinDelay;

            announcersQueue[levelTarget].Enqueue(new DelayData
            {
                perfectType = type,
                delay = delay,
                position = position
            });
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            foreach (var queue in announcersQueue.ToList())
            {
                if (announcersQueue[queue.Key].Count > 0)
                {
                    DelayData firstData = announcersQueue[queue.Key].First();

                    firstData.delay -= deltaTime;

                    if (firstData.delay <= 0.0f)
                    {
                        PlayAnnouncer(firstData.perfectType, firstData.position);
                        announcersQueue[queue.Key].Dequeue();
                    }
                }
            }
        }

        #endregion
    }
}
