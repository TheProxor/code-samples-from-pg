using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Pool;
using Drawmasters.Pool.Interfaces;
using Drawmasters.ServiceUtil;
using GameFlow.Extensions;

namespace Drawmasters.Levels
{
    public class LevelSkullAnnouncersController : ILevelController
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

        private const string headLimbName = "head";
        
        private AnnouncerSettings settings;

        private ComponentPool poolForObject;

        private readonly Dictionary<LevelTarget, Queue<DelayData>> announcersQueue =
            new Dictionary<LevelTarget, Queue<DelayData>>();

        #endregion


        #region Methods

        public void Initialize()
        {
            LevelProgressObserver.OnKillEnemy += LevelStateObserver_OnKillEnemy;
#warning hotfix
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            settings = IngameData.Settings.skullAnnouncerSettings;

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
            LevelProgressObserver.OnKillEnemy -= LevelStateObserver_OnKillEnemy;
        }


        private void PlayAnnouncer(PerfectType type, Vector3 position)
        {
            Sprite sprite = settings.GetSprite(type);
            if (sprite != null)
            {
                ShowAnnouncer(sprite, settings.lifeTime, position, settings.moveCurve, settings.alphaCurve, null);
            }
        }


        private void ShowAnnouncer(Sprite sprite, float lifeTime, Vector3 position, AnimationCurve moveCurve,
            AnimationCurve alphaCurve, AnimationCurve scaleCurve)
        {
            Announcer announcer = poolForObject.Pop() as Announcer;

            announcer.Init(poolForObject,
                sprite,
                position,
                position.SetY(position.y + settings.offsetY),
                lifeTime,
                moveCurve,
                alphaCurve,
                scaleCurve);
        }

        #endregion


        #region Events handlers

        private void LevelStateObserver_OnKillEnemy(LevelTarget levelTarget)
        {
            var proposal = GameServices.Instance.ProposalService.LeagueProposeController; 
            
            bool canShowAnnouncer = proposal.IsSkullsCollectAvailable && proposal.IsActive;
            if (!canShowAnnouncer)
            {
                return;
            }

            Vector3 position = levelTarget.FindLimbPosition(headLimbName, levelTarget.transform.position);

            if (!announcersQueue.ContainsKey(levelTarget))
            {
                announcersQueue.Add(levelTarget, new Queue<DelayData>());
            }

            announcersQueue[levelTarget].Enqueue(new DelayData
            {
                perfectType = PerfectType.LeagueKill,
                delay = settings.announcersMinDelay,
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