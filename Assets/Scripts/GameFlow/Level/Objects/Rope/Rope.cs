using System;
using System.Collections.Generic;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class Rope : ComponentLevelObject
    {
        #region Nested types

        [Serializable]
        public class SerializableData
        {
            public float length = default;
            public float segmentShift = default;
            public Vector3 endAnchorOffset = default;
            public Vector3 hookAnchorOffset = default;
            public bool isSphericalTrajectory = default;
        }

        #endregion



        #region Fields

        [SerializeField] private float emptySegmentsScale = default;
        [SerializeField] private float defaultSegmentsScale = default;

        [SerializeField] private Rigidbody2D hook = default;

        [SerializeField] private LineRenderer lineRenderer = default;

        private List<RopeComponent> components;

        #endregion



        #region Properties

        public SerializableData LoadedData { get; private set; }

        public Rigidbody2D Hook => hook;

        public float DefaultSegmentsScale => defaultSegmentsScale;

        #endregion



        #region Override methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            SerializableData CurrentData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);

            LoadedData = new SerializableData
            {
                length = CurrentData.length,
                segmentShift = CurrentData.segmentShift,
                endAnchorOffset = CurrentData.endAnchorOffset,
                hookAnchorOffset = CurrentData.hookAnchorOffset,
                isSphericalTrajectory = CurrentData.isSphericalTrajectory
            };
        }


        protected override void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<RopeComponent>
                {
                    new RopeStageComponent(),
                    new EmptyRopeComponent(emptySegmentsScale),
                    new RopeRendererComponent(lineRenderer),
                    new RopeCreateComponent()
                };
            }

            foreach (var component in components)
            {
                component.Initialize(this);
            }
        }


        protected override void EnableComponents()
        {
            foreach (var component in components)
            {
                component.Enable();
            }
        }


        protected override void DisableComponents()
        {
            foreach (var component in components)
            {
                component.Disable();
            }
            
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public override void FreeFallObject(bool isImmediately)
        {
            base.FreeFallObject(isImmediately);

            float delay = isImmediately ? 0.0f : IngameData.Settings.bossLevelSettings.objectFreeFallDelay;

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                Hook.simulated = false;
                Hook.bodyType = RigidbodyType2D.Dynamic;
                Hook.constraints = RigidbodyConstraints2D.None;
            }, delay);
        }

        #endregion
    }
}
