using System.Collections;
using Drawmasters.Levels;
using Spine;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorEnemyHitmastersBossLevelObject : EditorLevelObject
    {
        #region Fields

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;

        private BossSerializableData serializableData;

        #endregion



        #region Properties

        public int SkinIndex { get; set; }

        #endregion



        #region Methods

        protected override void Awake()
        {
            base.Awake();

            var animations = GetComponentsInChildren<SkeletonAnimation>(true);

            foreach (var skeletonAnimation in animations)
            {
                skeletonAnimation.Initialize(true);
                skeletonAnimation.LateUpdate();
            }
        }

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
                var loadedData = JsonUtility.FromJson<BossSerializableData>(data.additionalInfo);

                SkinIndex = loadedData.skinIndex;
            }

            string[] availableSkins = IngameData.Settings.bossLevelTargetSettings.skins;

            bool isCorrectIndex = (SkinIndex >= 0) &&
                                  (SkinIndex < availableSkins.Length);
            if (isCorrectIndex)
            {
                SetSkin(availableSkins[SkinIndex]);
            }
            else
            {
                CustomDebug.Log($"Incorrect sprite index in {gameObject.name}");
            }
        }


        public void SetSkin(string skinName)
        {
            if (serializableData != null)
            {
                serializableData.skinIndex = SkinIndex;
            }

            Skin foundSkin = skeletonAnimation.Skeleton.Data.FindSkin(skinName);

            if (foundSkin == null)
            {
                CustomDebug.Log($"Can't set skin {skinName}");
                return;
            }

            if (isSelected)
            {
                Deselect();
            }

            skeletonAnimation.skeleton.SetSkin(foundSkin);

            StartCoroutine(UpdateSelection());

            IEnumerator UpdateSelection()
            {
                yield return null;
                yield return null;

                RefreshRenderers();

                if (!isSelected)
                {
                    Select();
                }
            }
        }


        public override InspectorExtensionBase GetAdditionalInspector() 
            => GetAdditionalInspector("EnemyLevelObjectInspectorExtension.prefab");


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new BossSerializableData
                {
                    skinIndex = SkinIndex
                };
            }
        }

        public virtual void RefreshSerializableData()
        {
            if (serializableData != null)
            {
                serializableData.skinIndex = SkinIndex;
            }
        }

        #endregion
    }
}