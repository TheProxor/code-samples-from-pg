using Spine;
using Spine.Unity;
using Spine.Unity.Examples;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract partial class LevelTarget : ComponentLevelObjectColors, 
        ILaserDestroyable, 
        ILaserDestroyableCallback, 
        ICoinCollector
    {
        #region Fields

        public event Action<LevelTarget> OnHitted;
        public event Action<LevelTarget> OnDefeated;

        public event Action<LevelTarget> OnShouldApplyRagdoll;
        public event Action<bool> OnShouldSetImmortal;

        public event Action OnShouldStartLaserDestroy;

        private static readonly Vector3 RagdollPositionFinishGame = Vector3.one * -1000.0f;

        [SerializeField] protected Rigidbody2D standRigidbody = default;
        [SerializeField] protected List<Collider2D> standColliders = default;

        [SerializeField] protected List<LevelTargetLimbPart> limbsParts = default;
        [SerializeField] protected List<LevelTargetLimb> limbs = default;

        private List<string> choppedOffLimbs { get; set; } = new List<string>();

        #endregion



        #region Properties

        public abstract LevelTargetType Type { get; }

        public abstract bool AllowPerfects { get; }

        public abstract bool AllowVisualizeDamage { get; }

        public abstract Renderer Renderer { get; }

        public List<LevelTargetLimb> Limbs => limbs;

        public List<string> ExplodedLimbs { get; private set; } = new List<string>();

        public LevelTargetSettings Settings { get; private set; }

        public Rigidbody2D StandRigidbody
        {
            get => standRigidbody;
            set => standRigidbody = value;
        }

        public bool IsBoss => Type == LevelTargetType.Boss;

        public PreviousFrameRigidbody2D StandPreviousFrameRigidbody2D { get; private set; }

        public RagdollPreviousFrameBodies RigidbodyPairs { get; private set; }

        public abstract SkeletonAnimation SkeletonAnimation { get; }

        public abstract SkeletonRagdoll2D Ragdoll2D { get; }

        public Vector3 FocusPostion
        {
            get
            {
                Vector3 result = Vector3.zero;

                if (Ragdoll2D.IsActive)
                {
                    LevelTargetLimb aliveLimb = Limbs.Find(l => !l.IsChoppedOff);

                    if (aliveLimb != null)
                    {
                        Rigidbody2D rb = Ragdoll2D.GetRigidbody(aliveLimb.RootBoneName);

                        if (rb != null)
                        {
                            result = rb.position;
                        }
                    }
                }
                else if (StandRigidbody != null)
                {
                    result = StandRigidbody.position;
                }
                else
                {
                    result = transform.position;
                }

                return result;
            }
        }


        public override Vector3 CenterPosition
        {
            get
            {
                LevelTargetLimb aliveLimb = Limbs.Find(l => l.RootBoneName.Equals("head"));
                return aliveLimb == null ? base.CenterPosition : aliveLimb.transform.position;
            }
        }

        public bool IsHitted { get; protected set; }

        public bool IsImmortal { get; private set; }


#warning refactor need
        public List<Spikes> CurrentEnteredSpikes { get; private set; }


        private List<LevelTargetComponent> components;
        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            Settings = IngameData.Settings.levelTargetSettings;

            SkeletonAnimation.transform.localPosition = Vector3.zero;
            SkeletonAnimation.Skeleton.SetToSetupPose();
            SkeletonAnimation.Update(0);
            SkeletonAnimation.Skeleton.UpdateWorldTransform();

            base.SetData(data);
        }


        protected override void StartReturnToInitialState()
        {
            StandPreviousFrameRigidbody2D.Deinitialize();
            RigidbodyPairs.Deinitialize();

            if (Ragdoll2D.IsActive)
            {
                Ragdoll2D.SetSkeletonPosition(RagdollPositionFinishGame);
                Ragdoll2D.Remove();
            }

            base.StartReturnToInitialState();
        }


        protected override void FinishReturnToInitialState()
        {
            StandPreviousFrameRigidbody2D = new PreviousFrameRigidbody2D(standRigidbody);
            StandPreviousFrameRigidbody2D.Initialize();

            base.FinishReturnToInitialState();

            RigidbodyPairs = new RagdollPreviousFrameBodies(this);
            RigidbodyPairs.Initialize();

            choppedOffLimbs = new List<string>();
            ExplodedLimbs = new List<string>();
            CurrentEnteredSpikes = new List<Spikes>();
            IsHitted = false;
            SetImmortal(false);
        }


        protected abstract List<LevelTargetComponent> CreateComponents();


        protected override void InitializeComponents()
        {
            components = components ?? CreateComponents();

            limbs.ForEach(limb => limb.Initialize(this));
            components.ForEach(c => c.Initialize(this));
        }


        protected override void EnableComponents()
        {
            components.ForEach(c => c.Enable());
        }


        protected override void DisableComponents()
        {
            components.ForEach(c => c.Disable());
        }


        protected void InvokeHittedEvent() => OnHitted?.Invoke(this);


        protected void InvokeDefeatEvent() => OnDefeated?.Invoke(this);


        public virtual void MarkHitted()
        {
            if (!IsHitted)
            {
#if UNITY_EDITOR

                if (isDebugRagdoll)
                {
                    foreach (var slot in SkeletonAnimation.skeleton.Slots)
                    {
                        slot.SetColor(Color.red);
                    }
                }

#endif
                IsHitted = true;
                InvokeHittedEvent();
                InvokeDefeatEvent();
            }
        }


        public void SetImmortal(bool shouldSetImmortal)
        {
            OnShouldSetImmortal?.Invoke(shouldSetImmortal);
            IsImmortal = shouldSetImmortal;
        }

        public bool TryTeleport(PortalObject enteredPortal, PortalObject exitPortal)
        {
            bool wasTeleported = false;

            if (AllowToTeleport)
            {
                if (StandRigidbody != null)
                {
                    wasTeleported |= TeleportRigidbody(StandRigidbody, StandPreviousFrameRigidbody2D);
                }

                Rigidbody2D[] ragdollRBs = Ragdoll2D.RigidbodyArray;

                foreach (var i in ragdollRBs)
                {
                    if (i != null && !choppedOffLimbs.Contains(i.name))
                    {
                        wasTeleported |= TeleportRigidbody(i, RigidbodyPairs.GetPreviousRigidbody(i));
                    }
                }

                MarkTeleported();
            }

            if (wasTeleported)
            {
                //OnShouldTeleport?.Invoke(enteredPortal, exitPortal);
            }

            return wasTeleported;


            bool TeleportRigidbody(Rigidbody2D rigidbody2D, PreviousFrameRigidbody2D previousFrameRB)
            {
                if (rigidbody2D == null || previousFrameRB == null)
                {
                    return false;
                }

                bool isInBack = Vector3.Dot(previousFrameRB.Velocity, enteredPortal.transform.up) > 1.0f; // should be zero but  1.0f for precision

                if (!isInBack)
                {
                    float velocityMagnitude = previousFrameRB.Velocity.magnitude;

                    PortalsSettings portalsSettings = IngameData.Settings.portalsSettings;
                    float minVelocityMagnitude = portalsSettings.minLevelTargetVelocityMagnitude;
                    float maxVelocityMagnitude = portalsSettings.maxLevelTargetVelocityMagnitude;
                    velocityMagnitude = Mathf.Clamp(velocityMagnitude, minVelocityMagnitude, maxVelocityMagnitude);

                    rigidbody2D.velocity = exitPortal.transform.up.normalized * velocityMagnitude;

                    float portalExitOffset = IngameData.Settings.portalsSettings.levelTargetExitOffset;
                    rigidbody2D.position = exitPortal.transform.position.ToVector2() + portalExitOffset * exitPortal.transform.up.ToVector2();

                    return true;
                }

                return false;
            }
        }



        public Slot GetEnabledSlot(string boneName) =>
            SkeletonAnimation.skeleton.Slots.Find(s => s.GetColor().a > 0 && s.Data.BoneData.Name.Equals(boneName));


        public virtual void ApplyRagdoll()
        {
            StandPreviousFrameRigidbody2D.Deinitialize();

            OnShouldApplyRagdoll?.Invoke(this);
        }


        public void StartLaserDestroy() =>
            OnShouldStartLaserDestroy?.Invoke();


        public void AddChoppedOffLimb(string rootBoneName) =>
            choppedOffLimbs.Add(rootBoneName);


        public bool IsChoppedOffLimb(string rootBoneName) =>
            choppedOffLimbs.Contains(rootBoneName);


        public void MarkLimbExploded(string boneName) =>
            ExplodedLimbs.Add(boneName);
        

        public LevelTargetLimb GetLimb(Rigidbody2D rigidbody2D) =>
            limbs.Find(e => e.RootBoneName == rigidbody2D.name);


        protected override void RefreshVisualColor()
        {
            if (!ShouldLoadColorData)
            {
                CustomDebug.Log("Attempt to RefreshVisualColor when color data should be not loaded");
                return;
            }

            if (!IsColorsDataLoaded)
            {
                CustomDebug.Log($"Attempt to refresh colors visual before data loaded");
                return;
            }
        }

        #endregion



        #region ICoinCollector

        public Vector2 CurrentPosition =>
            FocusPostion.ToVector2();

        #endregion
    }
}
