using Drawmasters.Levels;
using UnityEngine;



namespace Drawmasters.LevelConstructor
{
    public class EditorVisualLevelObject : EditorLevelObject
    {
        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default;
        private BoxCollider2D mainCollider2D;

        private VisualLevelObject.SerializableData serializableData = default;

        #endregion



        #region Properties

        public int SpriteIndex { get; set; }


        public int SortingOrder { get; set; } = 5;


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


        private BoxCollider2D Collider2D
        {
            get
            {
                if (mainCollider2D == null)
                {
                    mainCollider2D = GetComponent<BoxCollider2D>();
                }

                return mainCollider2D;
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
                var loadedData = JsonUtility.FromJson<VisualLevelObject.SerializableData>(data.additionalInfo);

                SpriteIndex = loadedData.spriteIndex;
                SortingOrder = loadedData.sortingOrder;
            }

            Sprite[] availableSprites = IngameData.Settings.commonVisualObjectsSettings.FindSprites(EditorObjectsContainer.CurrentWeaponType);

            if (availableSprites != null &&
                SpriteIndex < availableSprites.Length)
            {
                Refresh(availableSprites[SpriteIndex]);
            }
        }


        public void Refresh(Sprite sprite)
        {
            SpriteRenderer.sprite = sprite;
            Collider2D.size = SpriteRenderer.sprite.bounds.size;

            SpriteRenderer.sortingOrder = SortingOrder;

            if (serializableData != null)
            {
                serializableData.spriteIndex = SpriteIndex;
                serializableData.sortingOrder = SortingOrder;
            }
        }


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("VisualLevelObjectInspectorExtension.prefab");


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new VisualLevelObject.SerializableData
                {
                    spriteIndex = SpriteIndex,
                    sortingOrder = SortingOrder
                };
            }
        }

        #endregion
    }
}
