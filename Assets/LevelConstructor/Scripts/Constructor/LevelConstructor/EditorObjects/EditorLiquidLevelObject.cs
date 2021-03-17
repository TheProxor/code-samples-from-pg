using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorLiquidLevelObject : EditorLevelObject
    {
        #region Fields

        [SerializeField] private BoxCollider2D boxCollider = default;

        private SpriteRenderer spriteRenderer;
        protected LiquidLevelObject.SerializableData serializableData;

        #endregion



        #region Properties

        public LiquidLevelObjectType Type { get; set; } = LiquidLevelObjectType.Acid;


        public Vector2 Size { get; set; }


        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponent<SpriteRenderer>();
                }

                return spriteRenderer;
            }
        }

        #endregion



        #region Unity lifecycle

        public void Update()
        {
            boxCollider.size = new Vector2(SpriteRenderer.size.x, boxCollider.size.y);
        }

        #endregion



        #region Methods

        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("LiquidInspectorExtension.prefab");


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
                var loadedData = JsonUtility.FromJson<LiquidLevelObject.SerializableData>(data.additionalInfo);

                Size = loadedData.size;
                Type = loadedData.type;
            }
            else
            {
                Size = SpriteRenderer.size;
            }

            RefreshData();
        }


        public void RefreshData()
        {
            SpriteRenderer.size = Size;

            if (serializableData != null)
            {
                serializableData.size = Size;
                serializableData.type = Type;
            }
        }


        protected override void LoadDefaultData()
        {
            if (serializableData == null)
            {
                serializableData = new LiquidLevelObject.SerializableData
                {
                    size = SpriteRenderer.size,
                    type = Type
                };
            }
        }

        #endregion
    }
}
