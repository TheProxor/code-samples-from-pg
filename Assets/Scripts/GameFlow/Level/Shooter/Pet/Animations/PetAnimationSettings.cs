using System;
using Spine.Unity;
using UnityEngine;
using static Drawmasters.Pets.PetLevelSettings;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "PetAnimationSettings",
                  menuName = NamingUtility.MenuItems.IngameSettings + "PetAnimationSettings")]
    public class PetAnimationSettings : ScriptableObject
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public PetSkinType type = default;

            [SpineAnimation(dataField = "dataAsset")] public string[] tapReactionAnimationNames = default;
            [SpineAnimation(dataField = "dataAsset")] public string[] tapReactionSleepAnimationNames = default;

            [SpineBone(dataField: "dataAsset")] public string fxInOutBoneName = default;
            [SpineBone(dataField: "dataAsset")] public string fxMagicBoneName = default;
        }


        [Serializable]
        public class MoveTypeData
        {
            public MoveType moveType = default;
            [SpineAnimation(dataField: "dataAsset")] public string[] moveAnimationNames = default;
        }

        #endregion



        #region Fields

#pragma warning disable 0414

        // for reflection only
        public SkeletonDataAsset dataAsset = default;

#pragma warning restore 0414

        [SerializeField] private Data[] animations = default;

        public float ingameTrackEnd = default;

        [Header("Level animations")]
        [SpineAnimation(dataField = "dataAsset")] public string moveAnimationKey = default;

        [Header("Base animations")]
        public float showOnSceneAnimationDelay = default;
        public float showOnLevelAnimationDelay = default;

        // TODO: rename on just hide
        [SpineAnimation(dataField = "dataAsset")] public string hideIdleName = default;

        [SpineAnimation(dataField = "dataAsset")] public string idleName = default;

        [SpineAnimation(dataField = "dataAsset")]
        // TODO: rename on appear
        [SerializeField] private string[] appearanceNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] disappearNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] mainMenuNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] ingameNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] forcemeterNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] storeNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] premiumStoreNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] shootAnimationNames = default;

        [Header("Movement")]
        [SerializeField] private MoveTypeData[] moveTypeData = default;

        [Header("Main menu")]
        
        [SerializeField]
        [SpineAnimation(dataField = "dataAsset")]
        private string[] emotionsAnimationNames = default;

        public float emotionsAnimationsDelay = default;

        [Header("Sleep")]
        [SpineAnimation(dataField = "dataAsset")]
        public string sleepStartAnimationName = default;
        public PetSkinType[] canPlaySleepStartPetsSkins = default;

        [SpineAnimation(dataField = "dataAsset")]
        public string sleepIdleAnimationName = default;

        [SpineAnimation(dataField = "dataAsset")]
        public string wakeUpAnimationName = default;

        [Header("Win")]

        [SerializeField]
        [SpineAnimation(dataField = "dataAsset")]
        private string[] ingameWinAnimationNames = default;

        #endregion


        #region Properties

        public string RandomAppearNames =>
            appearanceNames.RandomObject();


        public string RandomDisappearNames =>
            disappearNames.RandomObject();


        public string RandomMainMenuNames =>
            mainMenuNames.RandomObject();


        public string RandomIngameName =>
            ingameNames.RandomObject();


        public string RandomForcemeterNames =>
            forcemeterNames.RandomObject();


        public string RandomStoreNames =>
            storeNames.RandomObject();


        public string RandomPremiumStoreNames =>
            premiumStoreNames.RandomObject();


        public string RandomShootAnimationName =>
            shootAnimationNames.RandomObject();


        public string RandomEmotionsAnimationName =>
            emotionsAnimationNames.RandomObject();


        public string RandomIngameWinAnimationName =>
            ingameWinAnimationNames.RandomObject();

        #endregion


        #region Methods

        public bool CanPlaySleepStartAnimation(PetSkinType petSkinType) =>
            Array.Exists(canPlaySleepStartPetsSkins, e => e == petSkinType);


        public string FindMoveAnimationName(MoveType moveType)
        {
            MoveTypeData foundData = Array.Find(moveTypeData, e => e.moveType == moveType);
            return foundData == null ? string.Empty : foundData.moveAnimationNames.RandomObject();
        }


        public string FxInOutBoneName(PetSkinType type)
        {
            string result = string.Empty;
            Data data = animations.Find(x => x.type == type);
            if (data != null)
            {
                result = animations.Find(x => x.type == type)?.fxInOutBoneName;
            }
            if (string.IsNullOrEmpty(result))
            {
                CustomDebug.Log($"No FxInOutBoneName for type {type} in PetAnimationSettings");
            }

            return result;
        } 


        public string FxMagicBoneName(PetSkinType type)
        {
            string result = string.Empty;
            Data data = animations.Find(x => x.type == type);
            if (data != null)
            {
                result = data.fxMagicBoneName;
            }
            if (string.IsNullOrEmpty(result))
            {
                CustomDebug.Log($"No FxMagicBoneName for type {type} in PetAnimationSettings");
            }
            return result;
        }


        public string RandomTapReactionAnimationName(PetSkinType petSkinType)
        {
            Data data = animations.Find(x => x.type == petSkinType);
            return data == null || data.tapReactionAnimationNames.Length == 0 ? string.Empty : data.tapReactionAnimationNames.RandomObject();
        }


        public string RandomTapReactionSleepAnimationNames(PetSkinType petSkinType)
        {
            Data data = animations.Find(x => x.type == petSkinType);
            return data == null || data.tapReactionSleepAnimationNames.Length == 0 ? string.Empty : data.tapReactionSleepAnimationNames.RandomObject();
        }
        

        public string GetMainMenuAnimation(int index) =>
            GetAnimation(index, mainMenuNames);


        public string GetIngameAnimation(int index) =>
            GetAnimation(index, ingameNames);


        public string GetForcemeterAnimation(int index) =>
            GetAnimation(index, forcemeterNames);


        public string GetStoreAnimation(int index) =>
            GetAnimation(index, storeNames);

        private string GetAnimation(int index, string[] array)
        {
            string result = string.Empty;
            if (index >= 0 && index < array.Length)
            {
                result = array[index];
            }

            return result;
        }

        #endregion
    }
}
