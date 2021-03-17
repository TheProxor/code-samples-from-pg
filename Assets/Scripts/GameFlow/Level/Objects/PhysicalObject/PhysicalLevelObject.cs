using System;
using System.Collections.Generic;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class PhysicalLevelObject : ComponentLevelObject, 
        ILaserDestroyable, 
        ILaserDestroyableCallback, 
        ITeleportable
    {
        #region Nested types

        [Serializable]
        public class SerializableData
        {
            public int spriteIndex = default;
            public float width = default;
        }

        #endregion



        #region Fields

        public event Action<PhysicalLevelObject> OnPreDestroy;
        public event Action<PhysicalLevelObject> OnDestroy;

        public event Action<PortalObject, PortalObject> OnShouldTeleport;

        public event Action OnShouldStartLaserDestroy;

        [SerializeField] protected Collider2D mainCollider2D = default;
        [SerializeField] protected SpriteRenderer spriteRenderer = default;
        [SerializeField] private CollisionNotifier collisionNotifier = default;
        [SerializeField] private PhysicalLevelObjectData physicalData = default;
        [SerializeField] private List<PhysicalLevelObjectComponent> components = default;

        private JointsHandler jointsHandler;
        private SchedulerTask refreshJointsTask;

        private bool isDestroyed;

        private readonly PhysicalLevelObjectComponent[] componentsToAddOnStart = {
            new BossLevelEndEffectComponent(),
            new LaserDestroyComponent(),
            new BonusLevelBehaviourComponent(),
            new PhysicalObjectTeleportComponent(),
            new PhysicalObjectBonusLevelHighlightComponent(),
            new PhysicalObjectBonusLevelShineComponent()
        };

        private List<PhysicalLevelObjectComponent> necessaryComponents;  

        #endregion



        #region Properties

        protected List<PhysicalLevelObjectComponent> NecessaryComponents
        {
            get
            {
                if (necessaryComponents == null)
                {
                    necessaryComponents = new List<PhysicalLevelObjectComponent>();
                    necessaryComponents.AddRange(PhysicalLevelObjectComponentsFabric.GetNecessaryComponents(physicalData));
                }

                return necessaryComponents;
            }
        }

        public PhysicalLevelObjectData PhysicalData => physicalData;


        public SerializableData LoadedData { get; private set; }


        public PreviousFrameRigidbody2D PreviousFrameRigidbody2D { get; private set; }


        public Collider2D MainCollider2D => mainCollider2D;


        public SpriteRenderer SpriteRenderer => spriteRenderer;


        public float Strength { get; private set; }


        public bool IsLinkedObjectsPart { get; private set; }

        public bool IsOutOfZone { get; private set; }

        public override bool AllowToTeleport => base.AllowToTeleport &&
                                                IngameData.Settings.portalsSettings.AllowTeleport(PhysicalData);

        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            IsLinkedObjectsPart = data.isLinkedObjectsPart;

            PreviousFrameRigidbody2D = new PreviousFrameRigidbody2D(Rigidbody2D);

            SerializableData CurrentData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);

            LoadedData = new SerializableData
            {
                spriteIndex = CurrentData.spriteIndex,
                width = CurrentData.width,
            };

            Sprite[] availableSprites = IngameData.Settings.physicalObject.GetSprites(physicalData);

            if (LoadedData.spriteIndex < availableSprites.Length)
            {
                spriteRenderer.sprite = availableSprites[LoadedData.spriteIndex];
            }
            else
            {
                CustomDebug.Log($"No sprite with index {LoadedData.spriteIndex} for object with type {physicalData.type} and size type {physicalData.sizeType}");
            }

            // Do not create new jointsHandler instance, otherwise all data about default constraints will be lost.
            jointsHandler = jointsHandler ?? new JointsHandler(data.jointsPoints, Rigidbody2D); 
            jointsHandler.SetupJointPoints(data.jointsPoints);

            SetSortingLayer(RenderLayers.LevelObject);
        }


        public override void StartStageChange(StageLevelObjectData data, int stage)
        {
            base.StartStageChange(data, stage);

            refreshJointsTask = Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                RefreshJoints();
                refreshJointsTask = null;

            }, IngameData.Settings.bossLevelSettings.objectFreeFallDelay);
        }


        public override void FinishStageChange(int stage)
        {
            if (refreshJointsTask != null)
            {
                Scheduler.Instance.UnscheduleTask(refreshJointsTask);
                refreshJointsTask = null;
                RefreshJoints();
            }

            base.FinishStageChange(stage);
        }


        public override void ReturnToStage(StageLevelObjectData data, int stage)
        {
            base.ReturnToStage(data, stage);

            RefreshJoints();
        }


        protected override void FinishReturnToInitialState()
        {
            base.FinishReturnToInitialState();

            if (Rigidbody2D != null)
            {
                PreviousFrameRigidbody2D.Initialize();
            }

            jointsHandler?.Initialize();

            RefreshData();
            IsOutOfZone = false;
        }

        protected override void StartReturnToInitialState()
        {
            if (Rigidbody2D != null)
            {
                PreviousFrameRigidbody2D.Deinitialize();
            }

            jointsHandler?.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            refreshJointsTask = null;

            base.StartReturnToInitialState();
        }


        protected override void InitializeComponents()
        {
            components.AddRangeExclusive(componentsToAddOnStart);
            components.AddRangeExclusive(NecessaryComponents);

            //hotfix by maxim.ak
            if (components.Contains(i => i is BonusLevelAnimationComponent))
            {
                components.RemoveAll(i => i is LaserDestroyComponent);
            }

            components.ForEach(c => c.Initialize(collisionNotifier,
                                                 Rigidbody2D,
                                                 this));
        }


        protected override void EnableComponents() =>
            components.ForEach(c => c.Enable());


        protected override void DisableComponents() =>
            components.ForEach(c => c.Disable());


        public void AddImpuls(Vector3 pos, Vector2 impuls)
        {
            Rigidbody2D.AddForceAtPosition(impuls, pos, ForceMode2D.Impulse);
        }


        public virtual void DestroyObject()
        {
            if (!isDestroyed)
            {
                isDestroyed = true;

                OnPreDestroy?.Invoke(this);
                OnDestroy?.Invoke(this);
            }
        }


        public void MarkOutOfZone() => IsOutOfZone = true;

        protected override void OnFreeFall()
        {
            base.OnFreeFall();

            SetSortingLayer(RenderLayers.LevelObjectBossFall);
        }

        private void RefreshData()
        {
            if (LoadedData != null && Rigidbody2D != null)
            {
                Rigidbody2D.mass = IngameData.Settings.physicalObject.CalculateMass(physicalData);
                Rigidbody2D.gravityScale = IngameData.Settings.physicalObject.gravityScale;
                Strength = IngameData.Settings.physicalObject.CalculateStrength(physicalData);
            }

            isDestroyed = false;
        }


        private void RefreshJoints()
        {
            jointsHandler.Deinitialize();
            jointsHandler.SetupJointPoints(currentStageData.jointsPoints);
            jointsHandler.Initialize();
        }


        private void SetSortingLayer(string layerName)
        {
            foreach (var i in Renderers)
            {
                if (i != null)
                {
                    i.sortingLayerName = layerName;
                }
            }
        }


        public void StartLaserDestroy() =>
            OnShouldStartLaserDestroy?.Invoke();


        public bool TryTeleport(PortalObject enteredPortal, PortalObject exitPortal)
        {
            Vector2 previosFrameVelocity = PreviousFrameRigidbody2D.Velocity;

            bool isInBack = Vector3.Dot(previosFrameVelocity, enteredPortal.transform.up) > 1.0f; // should be zero but  1.0f for better precision

            if (!isInBack && AllowToTeleport)
            {
                OnShouldTeleport?.Invoke(enteredPortal, exitPortal);
                MarkTeleported();

                return true;
            }

            return false;
        }

        #endregion
    }
}
