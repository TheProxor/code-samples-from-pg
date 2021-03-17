using System.Collections.Generic;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorSpikes : EditorPhysicalLevelObject
    {
        #region Fields

        public const float TileStep = 13.4f;

        [SerializeField] private BoxCollider2D[] boxColliders = default;

        private List<EditorLevelObject> links;
        private Vector3 defaultLinkPosition;

        #endregion



        #region Properties

        public float Width { get; set; } = TileStep;


        public bool IsLinkedObjectsPart { get; set; }

        #endregion



        #region Unity lifecycle

        protected override void Awake()
        {
            base.Awake();

            if (boxColliders == null ||
                boxColliders.Length == 0)
            {
                boxColliders = GetComponentsInChildren<BoxCollider2D>();
            }

            LinkSettingRefreshed.Subscribe(OnLinksRefreshed);
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        private void OnDestroy()
        {
            LinkSettingRefreshed.Unsubscribe(OnLinksRefreshed);
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        private void Start()
        {
            OnLinksRefreshed();
        }


        public void Update()
        {
            foreach (var boxCollider in boxColliders)
            {
                boxCollider.size = new Vector2(SpriteRenderer.size.x, boxCollider.size.y);
            }
        }

        #endregion



        #region Methods

        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("SpikesInspectorExtension.prefab");


        public override LevelObjectData GetData()
        {
            LevelObjectData data = base.GetData();

            data.additionalInfo = JsonUtility.ToJson(serializableData);
            data.isLinkedObjectsPart = IsLinkedObjectsPart;

            return data;
        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            IsLinkedObjectsPart = data.isLinkedObjectsPart;

            if (!string.IsNullOrEmpty(data.additionalInfo))
            {
                var loadedData = JsonUtility.FromJson<PhysicalLevelObject.SerializableData>(data.additionalInfo);

                SpriteIndex = loadedData.spriteIndex;
                Width = Mathf.Max(TileStep, loadedData.width);
            }
            else
            {
                Width = TileStep;
            }

            RefreshSerializableData();
        }


        public override void RefreshSerializableData()
        {
            base.RefreshSerializableData();

            SpriteRenderer.size = new Vector2(Width, SpriteRenderer.size.y);

            if (serializableData != null)
            {
                serializableData.width = Width;
            }
        }


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new PhysicalLevelObject.SerializableData
                {
                    spriteIndex = SpriteIndex,
                    width = Width
                };
            }
            else
            {
                serializableData.width = Width;
            }
        }


        private void OnLinksRefreshed()
        {
            links = EditorLinker.GetLinks(this);

            if (links.Count > 0)
            {
                EditorLevelObject rootObject = links.FirstObject();

                defaultLinkPosition = rootObject.transform.position;
            }
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (links != null &&
                links.Count > 0)
            {
                EditorLevelObject rootObject = links.FirstObject();

                Vector3 offset = rootObject.transform.position - defaultLinkPosition;
                transform.position += offset;

                defaultLinkPosition = rootObject.transform.position;
            }
        }

        #endregion
    }
}
