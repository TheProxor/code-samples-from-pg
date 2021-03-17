using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using GameFlow.Extensions;


namespace Drawmasters.Levels
{
    public class LevelSoulTrailController : ILevelController
    {
        #region Fields

        private const string headLimbName = "head";
        
        private readonly Dictionary<LevelTarget, Queue<SoulTrail>> soulQueue =
            new Dictionary<LevelTarget, Queue<SoulTrail>>();

        public event Action OnTrailMoveFinish;
        public event Action OnQueueEmpty;
        
        #endregion


        #region Methods

        public void Initialize()
        {
            LevelProgressObserver.OnKillEnemy += LevelStateObserver_OnKillEnemy;
#warning hotfix
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            soulQueue.Clear();
        }


        public void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            LevelProgressObserver.OnKillEnemy -= LevelStateObserver_OnKillEnemy;
        }


        private void AddSoulTrail(LevelTarget levelTarget)
        {
            IngameScreen ingameScreen = UiScreenManager.Instance.LoadedScreen<IngameScreen>(ScreenType.Ingame);
            if (ingameScreen == null)
            {
                CustomDebug.Log($"No {nameof(IngameScreen)} was found. Starting appear pet from default position");
                return;
            }

            if (!ingameScreen.IsCallPetButtonAvailable ||
                !GameServices.Instance.PetsService.ChargeController.IsActive ||
                GameServices.Instance.LevelEnvironment.Context.Mode.IsHitmastersLiveOps())
            {
                return;
            }
            
            Transform parent = levelTarget.FindLimbTransform(headLimbName);

            SoulTrail trail = new SoulTrail(ingameScreen, parent);

            if (!soulQueue.ContainsKey(levelTarget))
            {
                soulQueue.Add(levelTarget, new Queue<SoulTrail>());
            }
            
            soulQueue[levelTarget].Enqueue(trail);
        }

        #endregion


        #region Events handlers

        private void LevelStateObserver_OnKillEnemy(LevelTarget levelTarget)
        {
            AddSoulTrail(levelTarget);
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (soulQueue.Count == 0)
            {
                return;
            }
            
            foreach (var queue in soulQueue.ToList())
            {
                if (soulQueue[queue.Key].Count > 0)
                {
                    SoulTrail firstData = soulQueue[queue.Key].First();
                    if (firstData.IsDone)
                    {
                        soulQueue[queue.Key].Dequeue();
                        firstData.Deinitialize();
                        OnTrailMoveFinish?.Invoke();
                    }
                }
            }

            if (!soulQueue.Any(x => x.Value.Count > 0))
            {
                OnQueueEmpty?.Invoke();
                soulQueue.Clear();
            }
        }

        #endregion
    }
}