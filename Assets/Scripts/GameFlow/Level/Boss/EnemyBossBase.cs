using System;
using System.Collections.Generic;
using Modules.General;
using Spine;
using Spine.Unity;
using Spine.Unity.Examples;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class EnemyBossBase : LevelTarget 
    {
        #region Fields

        public event Action OnAppeared;

        [SerializeField] protected SkeletonAnimation skeletonAnimation = default;
        [SerializeField] protected Renderer currentRenderer = default;
        [SerializeField] protected SkeletonRagdoll2D ragdoll2D = default;
        [SerializeField] protected BossHealthBar healthBar = default;
        [SerializeField] protected Transform centerRoot = default;

        [SerializeField] protected GameObject stageChangeRoot = default;
        [SerializeField] protected GameObject limbsRoot = default;

        protected bool isDefeated;        
        protected SchedulerTask stageChangeTask;        

        #endregion



        #region Abstract implementation

        public override sealed LevelTargetType Type => LevelTargetType.Boss;

        public override bool AllowPerfects => true;

        public override bool AllowVisualizeDamage => isDefeated;

        public override Renderer Renderer => currentRenderer;

        public override SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public override SkeletonRagdoll2D Ragdoll2D => ragdoll2D;

        #endregion



        #region Properties

        public Vector3 CenterPosition => centerRoot.position;

        #endregion



        #region Public methods

        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            isDefeated = false;
        }


        public override void FinishGame()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            stageChangeTask = null;
            
            base.FinishGame();
        }


        public void MarkDefeated()
        {
            IsHitted = true;
            isDefeated = true;

            Skin prevSkin = skeletonAnimation.Skeleton.Skin;
            
            skeletonAnimation.skeleton.SetSkin(SkinWithBoundingBoxesName);

            Ragdoll2D.RefreshTargetSkeleton();

            ApplyRagdoll();
            InvokeDefeatEvent();

            skeletonAnimation.Skeleton.SetSkin(prevSkin);
            skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        }


        public override void StartStageChange(StageLevelObjectData data, int stage)
        {
            base.StartStageChange(data, stage);

            stageChangeTask = Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                OnStateChanged();

                stageChangeTask = null;
            }, ObjectsFreeFallDelay);            
        }


        public override void FinishStageChange(int stage)
        {
            if (stageChangeTask != null)
            {
                Scheduler.Instance.UnscheduleTask(stageChangeTask);

                stageChangeTask = null;
            }
            base.FinishStageChange(stage);
        }


        public override void ReturnToStage(StageLevelObjectData data, int stage)
        {
            base.ReturnToStage(data, stage);

            SetupStageData();
        }


        public override void MarkHitted() 
            => InvokeHittedEvent();


        public void SetLimbsEnabled(bool enabled)
        {
            CommonUtility.SetObjectActive(limbsRoot, enabled);
            CommonUtility.SetObjectActive(stageChangeRoot, !enabled);
        }


        public void SetSlotsColor(Color value)
        {
            Slot[] enabledSlots = SkeletonAnimation.skeleton.Slots.ToArray();

            foreach (var slot in enabledSlots)
            {
                slot.SetColor(value);
            }
        }

        #endregion



        #region Abstract methods

        protected abstract float ObjectsFreeFallDelay { get; }

        protected abstract string SkinWithBoundingBoxesName { get; }

        protected abstract List<string> AvailableSkins { get; }        

        protected abstract void OnStateChanged();

        #endregion



        #region Protected methods

        protected void ApplySkin(int skinIndex)
        {
            if (skinIndex < AvailableSkins.Count)
            {
                skeletonAnimation.Initialize(true);

                Skin foundSkin = skeletonAnimation.Skeleton.Data.FindSkin(AvailableSkins[skinIndex]);
                if (foundSkin != null)
                {
                    skeletonAnimation.Skeleton.SetSkin(foundSkin);
                    skeletonAnimation.Skeleton.SetSlotsToSetupPose();
                    skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
                }
                else
                {
                    CustomDebug.Log($"Cannot find skin. Index : {skinIndex}, name : {AvailableSkins[skinIndex]}");
                }
            }
            else
            {
                CustomDebug.Log($"Cannot apply skin. Index is less than collection size. Index : {skinIndex}");
            }
        }


        protected void SetupStageData()
        {
            transform.position = currentStageData.position;
            transform.eulerAngles = currentStageData.rotation;
            StandRigidbody.bodyType = RigidbodyType2D.Static;

            Physics2D.SyncTransforms();
        }

        #endregion


        #region Events invocation

        protected void InvokeOnAppearEvent()
            => OnAppeared?.Invoke();

        #endregion



        #region Private methods


        #endregion
    }
}

