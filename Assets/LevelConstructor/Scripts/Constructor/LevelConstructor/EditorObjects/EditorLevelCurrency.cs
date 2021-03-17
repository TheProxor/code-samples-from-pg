using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorLevelCurrency : EditorLevelObject
    {
        #region Fields

        [SerializeField] private MeshRenderer objectRenderer = default;
        [SerializeField] private MeshFilter objectMesh = default;

        private CurrencyType currencyType = CurrencyType.Simple;
        protected CurrencyLevelObject.SerializableData serializableData;

        #endregion



        #region Properties

        public CurrencyType CurrencyType
        {
            get => currencyType;
            set
            {
                if (currencyType != value)
                {
                    SwitchModel(value);
                    currencyType = value;
                }
            }
        }


        public float CurrencyCount { get; set; }

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

            if (!string.IsNullOrEmpty(data.additionalInfo))
            {
                var loadedData = JsonUtility.FromJson<CurrencyLevelObject.SerializableData>(data.additionalInfo);

                CurrencyType = loadedData.CurrencyType;
                CurrencyCount = loadedData.CurrencyCount;
            }

            ResetRotation();
        }


        private void ResetRotation()
        {
            var rotation = transform.rotation;
            rotation.y = 0;
            transform.rotation = rotation;
        }


        private void SwitchModel(CurrencyType type)
        {
            if (serializableData == null)
            {
                LoadDefaultData();
            }

            serializableData.CurrencyType = type;

            CoinLevelObjectSettings settings = IngameData.Settings.coinLevelObjectSettings;

            objectMesh.mesh = settings.FindMesh(type);
            objectRenderer.material = settings.FindMaterial(type);
        }


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new CurrencyLevelObject.SerializableData
                {
                    CurrencyType = CurrencyType,
                    CurrencyCount = CurrencyCount
                };
            }
        }


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("CurrencyLevelObjectInspectorExtension.prefab");


        public virtual void RefreshSerializableData()
        {
            if (serializableData != null)
            {
                serializableData.CurrencyType = CurrencyType;
                serializableData.CurrencyCount = CurrencyCount;
            }
        }

        #endregion
    }
}
