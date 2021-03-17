using Drawmasters.Levels;
using Drawmasters.Levels.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorDynamicPlank : EditorLevelObject
    {
        #region Fields

        [SerializeField] private Rigidbody2D body = null;
        
        private DynamicPlank.SerializableData serializableData = default;

        #endregion



        #region Properties

        public DynamicPlank.CycleType CycleType { get; set; } = DynamicPlank.CycleType.None;

        public float Speed { get; set; } = 1f;

        public List<DynamicPlank.PathPoint> Path { get; set; } = new List<DynamicPlank.PathPoint>();

        #endregion



        #region Methods

        public override LevelObjectData GetData()
        {
            LevelObjectData data = base.GetData();

            data.additionalInfo = JsonUtility.ToJson(serializableData);

            return data;
        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            DynamicPlank.SerializableData loadedData =
                JsonUtility.FromJson<DynamicPlank.SerializableData>(data.additionalInfo);

            CycleType = loadedData.cycleType;
            Speed = loadedData.speed;
            Path = loadedData.path;
        }


        public override void Move(Vector3 newPosition)
        {
            base.Move(newPosition);

            if (!IsLocked)
            {
                StartCoroutine(SynchronizePosition());
            }
        }


        public override void Rotate(Vector3 newRotation)
        {
            if (!IsLocked)
            {
                Rigidbody2D.MoveRotation(newRotation.z);
                transform.Rotate(newRotation);
            }
        }


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("DynamicPlankInspectorExtension.prefab");


        protected override void GetRigidBody()
        {
            Rigidbody2D = body;
        }


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new DynamicPlank.SerializableData
                {
                    cycleType = CycleType,
                    speed = Speed,
                    path = Path
                };
            }
        }


        private IEnumerator SynchronizePosition()
        {
            yield return null;
            transform.position = Rigidbody2D.position;
        }

        #endregion
    }
}
