using Drawmasters.Effects;
using Spine.Unity;
using System.Collections.Generic;
using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ShooterIdleFxsAbstractComponent : ShooterComponent
    {
        #region Fields

        private readonly List<EffectHandler> effectHandlers;
        private readonly List<GameObject> effectRoots;
        private readonly PlayerData playerData;

        (string fxKey, string boneName)[] fxKeyAndBoneName;
        private bool shouldPlayOnStart;

        #endregion



        #region Properties

        protected abstract bool ShouldEnable { get; }

        #endregion



        #region Class lifecycle

        public ShooterIdleFxsAbstractComponent()
        {
            playerData = GameServices.Instance.PlayerStatisticService.PlayerData;

            effectRoots = new List<GameObject>();
            effectHandlers = new List<EffectHandler>();
        }

        #endregion



        #region Methods

        public override void StartGame()
        {
            if (ShouldEnable)
            {
                FillData();

                if (shouldPlayOnStart)
                {
                    PlayFxs();
                }
            }

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            playerData.OnShooterSkinSetted += PlayerData_OnShooterSkinSetted;
        }


        public override void Deinitialize()
        {
            StopFxs();

            if (!shouldPlayOnStart)
            {
                ClearFxRoots();
            }

            playerData.OnShooterSkinSetted -= PlayerData_OnShooterSkinSetted;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
        }


        private void PlayFxs()
        {
            if (!ShouldEnable)
            {
                return;
            }

            foreach (var (fxKey, boneName) in fxKeyAndBoneName)
            {
                string goName = string.Concat(boneName, "Fx");
                GameObject effectRoot = effectRoots.Find(e => e.name == goName);
                Transform root = effectRoot == null ? shooter.transform : effectRoot.transform;

                var effectHandler = EffectManager.Instance.CreateSystem(fxKey,
                                                                    true,
                                                                    default,
                                                                    default,
                                                                    root, TransformMode.Local);
                if (effectHandler != null)
                {
                    effectHandler.Play(withClear: false);
                }
            }
        }


        private void StopFxs()
        {
            foreach (var handler in effectHandlers)
            {
                EffectManager.Instance.ReturnHandlerToPool(handler);
            }

            effectHandlers.Clear();
        }


        private void ClearFxRoots()
        {
            foreach (var effectRoot in effectRoots)
            {
                if (effectRoot != null)
                {
                    Content.Management.DestroyObject(effectRoot);
                }
            }

            effectRoots.Clear();
        }


        protected abstract (string fxKey, string boneName)[] GetFxKeyAndBoneNames();

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.Initialized:
                    StopFxs();
                    PlayFxs();
                    break;

                case LevelState.ReturnToInitial:
                    shouldPlayOnStart = true;
                    break;

                default:
                    break;
            }
        }


        private void PlayerData_OnShooterSkinSetted()
        {
            if (ShouldEnable)
            {
                FillData();
                StopFxs();
                PlayFxs();
            }
            else
            {
                ClearFxRoots();
                StopFxs();
            }
        }


        private void FillData()
        {
            fxKeyAndBoneName = GetFxKeyAndBoneNames();

            foreach (var data in fxKeyAndBoneName)
            {
                bool isBoneExists = !string.IsNullOrEmpty(data.boneName) && shooter.SkeletonAnimation.skeleton.FindBone(data.boneName) != null;

                if (isBoneExists)
                {
                    GameObject effectRoot = SpineUtility.InstantiateBoneFollower(shooter.SkeletonAnimation, data.boneName, shooter.SkeletonAnimation.transform);
                    effectRoots.Add(effectRoot);
                }
            }

        }

        #endregion
    }
}
