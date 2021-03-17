using UnityEngine;
using System;
using Spine.Unity;
using Drawmasters.Levels;
using Drawmasters.Announcer;
using Drawmasters.Utils;
using System.IO;


namespace Drawmasters.Ui
{
    [CreateAssetMenu(fileName = "PetUiSettings",
        menuName = NamingUtility.MenuItems.IngameSettings + "PetUiSettings")]
    public class PetUiSettings : ScriptableObjectData<PetUiSettings.Data, PetSkinType>, IContentImport
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<PetSkinType>
        {
            public SkeletonDataAsset skeletonDataAsset = default;

            [Enum(typeof(EffectKeys))] public string idleFxKey = default;
            [SpineBone(dataField = "skeletonDataAsset")] public string idleFxKeyBoneName = default;

            public Sprite sprite = default;
            public Sprite smallBarSprite = default;

            [Header("Announcer")]
            public string unboughtAnnouncerKey = default;
        }

        #endregion



        #region Fields

        [Header("Announcer")]
        public MonopolyCurrencyAnnouncer petUnboughtAnnouncerPrefab = default;

        public Vector3 unboughtAnnouncerOffset = default;
        public CommonAnnouncer.Data unboughtAnnouncerData = default;
        public Sprite unboughtAnnouncerBackgroundSprite = default;

        #endregion



        #region Methods

        public SkeletonDataAsset FindMainMenuSkeletonData(PetSkinType petSkinType)
        {
            var foundData = FindData(petSkinType);

            if (foundData == null)
            {
                foundData = FindData(PetSkinType.PartyCat);
                CustomDebug.Log($"No animation for {petSkinType}. Attempt to return fallback cat animation.");
            }

            return foundData == null ? default : foundData.skeletonDataAsset;
        }


        public Sprite FindMainMenuSprite(PetSkinType petSkinType)
        {
            var foundData = FindData(petSkinType);

            if (foundData == null)
            {
                foundData = FindData(PetSkinType.PartyCat);
                CustomDebug.Log($"No animation for {petSkinType}. Attempt to return fallback cat animation.");
            }

            return foundData == null ? default : foundData.sprite;
        }


        public Sprite FindSmallBarSprite(PetSkinType petSkinType)
        {
            var foundData = FindData(petSkinType);

            if (foundData == null)
            {
                foundData = FindData(PetSkinType.PartyCat);
                CustomDebug.Log($"No animation for {petSkinType}. Attempt to return fallback cat animation.");
            }

            return foundData == null ? default : foundData.smallBarSprite;
        }
        


        public string FindMainMenuIdleFxKey(PetSkinType petSkinType)
        {
            var foundData = FindData(petSkinType);

            if (foundData == null)
            {
                foundData = FindData(PetSkinType.PartyCat);
                CustomDebug.Log($"No animation for {petSkinType}. Attempt to return fallback cat animation.");
            }

            return foundData == null ? default : foundData.idleFxKey;
        }


        public string FindMainMenuBoneName(PetSkinType petSkinType)
        {
            var foundData = FindData(petSkinType);

            if (foundData == null)
            {
                foundData = FindData(PetSkinType.PartyCat);
                CustomDebug.Log($"No animation for {petSkinType}. Attempt to return fallback cat animation.");
            }

            return foundData == null ? default : foundData.idleFxKeyBoneName;
        }


        public string FindUnboughtAnnouncerKey(PetSkinType petSkinType)
        {
            var foundData = FindData(petSkinType);

            if (foundData == null)
            {
                foundData = FindData(PetSkinType.PartyCat);
                CustomDebug.Log($"No data for {petSkinType}. Attempt to return fallback cat animation.");
            }

            string foundKey = foundData == null ? default : foundData.unboughtAnnouncerKey;

            bool isBonusLevelTutorialAvailable = false; // TODO: ADD AB TEST YUIRII.S
            const string bonusLevelTutorialReplaceKey = "loc_unbought_pet_tournament";
            const string bonusLevelTutorialKey = "loc_unbought_pet_more";
            bool isBonusLevelTutorialKey = foundKey.Equals(bonusLevelTutorialKey, StringComparison.Ordinal);

            string result;
            if (isBonusLevelTutorialKey)
            {
                result = isBonusLevelTutorialAvailable ? foundKey : bonusLevelTutorialReplaceKey;
            }
            else
            {
                result = foundKey;
            }

            return result;
        }

        #endregion



        #region IContentImport

        public async void ReimportContent()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 977083876;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (result) =>
            {
                ParseDataFromString(result);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            });
        }


        private void ParseDataFromString(string value)
        {
            using (var reader = new StringReader(value))
            {
                string line = reader.ReadLine(); // skip header
                line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    string[] values = CSVParseUtility.ParseRowsToArray(line);

                    if (Enum.TryParse(values[0], out PetSkinType skinType))
                    {
                        Data existedData = data.Find(e => e.key == skinType);
                        Data dataToAdd = existedData ?? new Data();

                        dataToAdd.key = skinType;
                        dataToAdd.unboughtAnnouncerKey = values[29];

                        data = data.Put(dataToAdd, e => e.key == skinType); // Temp Put.
                    }
                    else
                    {
                        Debug.Log($"<color=red>Parse error. Can't find {nameof(PetSkinType)} {values[0]}" +
                            $"Did you forget to add {nameof(PetSkinType)}? </color>");
                    }

                    line = reader.ReadLine();
                }
            }

            CustomDebug.Log($"<color=green> Reimorted data for {name} </color>");
        }

        #endregion
    }
}
