using System.Collections.Generic;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorRope : EditorLevelObject
    {
        #region Fields

        protected Rope.SerializableData serializableData;
        private List<EditorLevelObject> links;

        private Transform mainConnected;
        private Transform hookConnected;

        #endregion



        #region Properties

        public Vector3 HookConnectedObjectPosition => (hookConnected == null) ? Vector3.zero : hookConnected.position;


        public Vector3 ConnectedObjectPosition => (mainConnected == null) ? Vector3.zero : mainConnected.position;


        public float Length { get; set; } = 100.0f;


        public float SegmentsShift { get; set; } = 10.0f;


        public bool IsSphericalTrajectory { get; set; }


        public Vector3 HookAnchorOffset { get; set; } = Vector3.zero;


        public Vector3 EndAnchorOffset { get; set; } = Vector3.zero;


        public float ActualRopeLength => Vector2.Distance(HookConnectedObjectPosition - HookAnchorOffset, ConnectedObjectPosition - EndAnchorOffset);

        #endregion



        #region Unity lifecycle

        protected override void Awake()
        {
            base.Awake();

            LinkSettingRefreshed.Subscribe(OnLinksRefreshed);
        }


        private void OnDestroy()
        {
            LinkSettingRefreshed.Unsubscribe(OnLinksRefreshed);
        }


        private void Start()
        {
            OnLinksRefreshed();
        }

        #endregion



        #region Methods

        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("RopeInspectorExtension.prefab");


        public override LevelObjectData GetData()
        {
            LevelObjectData data = base.GetData();

            data.additionalInfo = JsonUtility.ToJson(serializableData);

            return data;
        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            if (!string.IsNullOrEmpty(data.additionalInfo))
            {
                var loadedData = JsonUtility.FromJson<Rope.SerializableData>(data.additionalInfo);

                Length = loadedData.length;
                SegmentsShift = loadedData.segmentShift;
                EndAnchorOffset = loadedData.endAnchorOffset;
                HookAnchorOffset = loadedData.hookAnchorOffset;
                IsSphericalTrajectory = loadedData.isSphericalTrajectory;
            }

            RefreshData();
        }


        public void RefreshData()
        {
            if (serializableData != null)
            {
                serializableData.length = Length;
                serializableData.segmentShift = SegmentsShift;
                serializableData.endAnchorOffset = EndAnchorOffset;
                serializableData.hookAnchorOffset = HookAnchorOffset;
                serializableData.isSphericalTrajectory = IsSphericalTrajectory;
            }
        }


        protected override void LoadDefaultData()
        {
            if (serializableData == null)
            {
                serializableData = new Rope.SerializableData
                {
                    length = Length,
                    segmentShift = SegmentsShift,
                    endAnchorOffset = EndAnchorOffset,
                    hookAnchorOffset = HookAnchorOffset,
                    isSphericalTrajectory = IsSphericalTrajectory
                };
            }
        }

        #endregion



        #region Events handlers

        private void OnLinksRefreshed()
        {
            links = EditorLinker.GetLinks(this);

            if (links.Count > 0)
            {
                mainConnected = links[0].transform;
            }

            if (links.Count > 1)
            {
                hookConnected = links[1].transform;
            }
        }

        #endregion
    }
}
