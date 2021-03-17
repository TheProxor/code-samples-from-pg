using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorPhysicalLevelObject : EditorLevelObject
    {
        #region Fields

        [SerializeField] private SpriteRenderer explosionAreaRenderer = default;

        private SpriteRenderer spriteRenderer;

        protected PhysicalLevelObject.SerializableData serializableData = default;

        #endregion



        #region Properties

        public int SpriteIndex { get; set; }


        protected SpriteRenderer SpriteRenderer
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
                var loadedData = JsonUtility.FromJson<PhysicalLevelObject.SerializableData>(data.additionalInfo);

                SpriteIndex = loadedData.spriteIndex;
            }

            PhysicalLevelObject referenceObject = Content.Storage.GetLevelObject(Index) as PhysicalLevelObject;

            if (referenceObject != null)
            {
                Sprite[] availableSprites = IngameData.Settings.physicalObject.GetSprites(referenceObject.PhysicalData);

                bool isCorrectIndex = (SpriteIndex >= 0) &&
                                      (SpriteIndex < availableSprites.Length);
                if (isCorrectIndex)
                {
                    SetSprite(availableSprites[SpriteIndex]);
                }
                else
                {
                    CustomDebug.Log($"Incorrect sprite index in {gameObject.name}");
                }

                if (IngameData.Settings.dynamiteSettings.IsExplosionDataExists(referenceObject.PhysicalData) &&
                    IngameData.Settings.dynamiteSettings.TryFindExplosionData(referenceObject.PhysicalData,
                                                        out DynamiteSettings.ExplosionData explosionData))
                {
                    if (explosionAreaRenderer == null)
                    {
                        explosionAreaRenderer = Instantiate(IngameData.Settings.dynamiteSettings.constructorVisualRenderer, transform);
                    }

                    explosionAreaRenderer.size = Vector2.one * explosionData.radius * 2.0f;
                }
            }
        }


        public void SetSprite(Sprite sprite)
        {
            SpriteRenderer.sprite = sprite;

            if (serializableData != null)
            {
                serializableData.spriteIndex = SpriteIndex;
            }
        }


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("PhysicalLevelObjectInspectorExtension.prefab");


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new PhysicalLevelObject.SerializableData
                {
                    spriteIndex = SpriteIndex
                };
            }
        }

        public virtual void RefreshSerializableData()
        {
            if (serializableData != null)
            {
                serializableData.spriteIndex = SpriteIndex;
            }
        }

        #endregion
    }
}
