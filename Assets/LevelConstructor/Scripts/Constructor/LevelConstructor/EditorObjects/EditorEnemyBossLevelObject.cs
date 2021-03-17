using System.Collections;
using Drawmasters.Levels;
using Spine;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorEnemyBossLevelObject : EditorLevelTargetObject
    {
        #region Fields

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;

        private AttackedBossSerializableData serializableData;

        #endregion



        #region Properties

        public int SkinIndex { get; set; }

        public RocketLaunchData[] RocketLaunchData { get; set; }

        public LevelObjectMoveSettings[] StagesMovement { get; set; }

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
                var loadedData = JsonUtility.FromJson<AttackedBossSerializableData>(data.additionalInfo);

                SkinIndex = loadedData.skinIndex;
                RocketLaunchData = loadedData.rocketLaunchData;
                StagesMovement = loadedData.stagesMovement;
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
                serializableData.rocketLaunchData = RocketLaunchData;
                serializableData.stagesMovement = StagesMovement;
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


        public override InspectorExtensionBase GetAdditionalInspector() => GetAdditionalInspector("BossLevelTargetInspectorExtension.prefab");


        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            if (serializableData == null)
            {
                serializableData = new AttackedBossSerializableData
                {
                    skinIndex = SkinIndex,
                    rocketLaunchData = RocketLaunchData,
                    stagesMovement = StagesMovement
                };
            }
        }

        public virtual void RefreshSerializableData()
        {
            if (serializableData != null)
            {
                serializableData.skinIndex = SkinIndex;
                serializableData.rocketLaunchData = RocketLaunchData;
                serializableData.stagesMovement = StagesMovement;
            }
        }


        protected override string GetSkin(ShooterColorType shooterColorType) =>
            IngameData.Settings.bossLevelTargetSettings.skinWithBoundingBoxes;
        
        #endregion
    }
}
 