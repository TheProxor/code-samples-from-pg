using System;
using System.Collections.Generic;
using System.IO;
using Drawmasters.Levels;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "WeaponSkinSettings",
                   menuName = NamingUtility.MenuItems.IngameSettings + "WeaponSkinSettings")]
    public class WeaponSkinSettings : ScriptableObject, IContentImport, IMenuItemRefreshable
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            [Space]
            public WeaponSkinType skinType = default;
            public WeaponType weaponType = default;
            public ShooterAimingType aimingType = default;
            public WeaponAnimationType weaponAnimationType = default;
        }


        [Serializable]
        public class PriceInfo
        {
            public WeaponSkinType type = default;
            [Min(0)] public float price = default;
        }


        [Serializable]
        public class VisualInfo
        {
            public WeaponSkinType type = default;

            public Sprite uiSprite = default;
            public Sprite uiOutlineSprite = default;
            public Sprite uiOutlineDisabledSprite = default;

            public Sprite uiResultSprite = default;
        }

        #endregion



        #region Fields

#pragma warning disable 0414

        [Tooltip("only for reflection")]
        [SerializeField] private SkeletonDataAsset asset = default;

#pragma warning restore

        [SerializeField] private Data[] data = default;
        [SerializeField] private PriceInfo[] priceInfo = default;

        public VisualInfo[] visualInfo = default;

        #endregion



        #region Methods

        public WeaponAnimationType GetAnimationType(WeaponSkinType type)
        {
            Data foundInfo = FindData(type);
            return foundInfo == null ? default : foundInfo.weaponAnimationType;
        }


        public float FindPrice(WeaponSkinType type)
        {
            PriceInfo foundInfo = Array.Find(priceInfo, e => e.type == type);
            return foundInfo == null ? default : foundInfo.price;
        }


        public WeaponType GetWeaponType(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? default : foundData.weaponType;
        }


        public ShooterAimingType GetAimingType(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? default : foundData.aimingType;
        }


        public Sprite GetSkinUiSprite(WeaponSkinType type)
        {
            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null ? default : foundInfo.uiSprite;
        }



        public Sprite GetUiOutlineSprite(WeaponSkinType type)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null || foundInfo.uiOutlineSprite == null ? fallbackSprite : foundInfo.uiOutlineSprite;
        }


        public Sprite GetUiOutlineDisabledSprite(WeaponSkinType type)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null || foundInfo.uiOutlineDisabledSprite == null ? fallbackSprite : foundInfo.uiOutlineDisabledSprite;
        }


        public bool TryGetSkinUiResultSprite(WeaponSkinType type, out Sprite uiResultSprite)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);

            bool isInfoExists = foundInfo != null || foundInfo.uiResultSprite != null;
            uiResultSprite = isInfoExists ? foundInfo.uiResultSprite : fallbackSprite;

            return uiResultSprite != null;
        }


        private Data FindData(WeaponSkinType skinType)
        {
            Data foundData = Array.Find(data, e => e.skinType == skinType);

            if (foundData == null)
            {
                CustomDebug.Log($"No data found for skin type {skinType} in {this}");
            }

            return foundData;
        }

        #endregion



        #region IContentImport

        public async void RefreshFromMenuItem()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 1019019582;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (result) =>
            {
                FillSkinsTypeEnum(result);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.Refresh();
#endif
            });
        }

        public async void ReimportContent()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 1019019582;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (result) =>
            {
                data = Array.Empty<Data>();
                visualInfo = Array.Empty<VisualInfo>();
                ParseDataFromString(result);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            });
        }


        private void FillSkinsTypeEnum(string value)
        {
            const string SkinsTypesFilePath = "Assets/Scripts/GameFlow/Shop/Skins/WeaponSkin/WeaponSkinType.cs";
            const string WriteEnumFormat = "{0} = {1},";
            int enumIndex = default;

            if (File.Exists(SkinsTypesFilePath))
            {
                File.Delete(SkinsTypesFilePath);
            }

            using (StreamWriter outfile = new StreamWriter(SkinsTypesFilePath))
            {
                outfile.WriteLine("namespace Drawmasters");
                outfile.WriteLine("{");
                outfile.WriteLine("    public enum WeaponSkinType");
                outfile.WriteLine("    {");
                outfile.WriteLine($"        {string.Format(WriteEnumFormat, "None", enumIndex)}");
                enumIndex++;

                using (var reader = new StringReader(value))
                {
                    string line = reader.ReadLine(); // skip header
                    line = reader.ReadLine();

                    while (!string.IsNullOrEmpty(line))
                    {
                        string[] values = CSVParseUtility.ParseRowsToArray(line);

                        outfile.WriteLine($"        {string.Format(WriteEnumFormat, values[0], enumIndex)}");
                        enumIndex++;

                        line = reader.ReadLine();
                    }
                }
                outfile.WriteLine("    }");
                outfile.WriteLine("}");

                outfile.Flush();
                outfile.Close();
            }
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

                    if (Enum.TryParse(values[0], out WeaponSkinType weaponSkinType))
                    {
                        Data dataToAdd = new Data();
                        dataToAdd.skinType = weaponSkinType;
                        dataToAdd.weaponType = (WeaponType)Enum.Parse(typeof(WeaponType), values[8]);
                        dataToAdd.aimingType = (ShooterAimingType)Enum.Parse(typeof(ShooterAimingType), values[9]);
                        dataToAdd.weaponAnimationType = (WeaponAnimationType)Enum.Parse(typeof(WeaponAnimationType), values[10]);

                        data = CommonUtility.Put(data, dataToAdd, e => e.skinType == weaponSkinType);

                        VisualInfo infoToAdd = new VisualInfo();
                        infoToAdd.type = weaponSkinType;

                        List<Sprite> allUiSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/Icons/Weapons");
                        infoToAdd.uiSprite = allUiSprites.Find(e => e.name.Equals(values[12], StringComparison.Ordinal));

                        List<Sprite> allUiOutlineSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/OutlineIcons/Weapons");
                        infoToAdd.uiOutlineSprite = allUiOutlineSprites.Find(e => e.name.Equals(values[21], StringComparison.Ordinal));
                        infoToAdd.uiOutlineDisabledSprite = allUiOutlineSprites.Find(e => e.name.Equals(values[22], StringComparison.Ordinal));

                        List<Sprite> allUiResultSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/OutlineIcons/ResultScreen");
                        infoToAdd.uiResultSprite = allUiResultSprites.Find(e => e.name.Equals(values[23], StringComparison.Ordinal));

                        visualInfo = visualInfo.Add(infoToAdd);
                    }
                    else
                    {
                        Debug.Log($"<color=red>Parse error. Can't find weaponskin type {values[0]}" +
                            $"Are you forget to add skin type? </color>");
                    }

                    line = reader.ReadLine();
                }
            }

            CustomDebug.Log($"<color=green> Reimorted data for {name} </color>");
        }

        #endregion
    }
}
