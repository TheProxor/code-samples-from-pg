using UnityEngine;
using System;
using Drawmasters.Levels;
using Spine.Unity;
using Drawmasters.LevelTargetObject;
using System.Collections.Generic;
using Drawmasters.Utils;
using System.IO;
using System.Linq;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "ShooterSkinsSettings",
                    menuName = NamingUtility.MenuItems.IngameSettings + "ShooterSkinsSettings")]
    public class ShooterSkinsSettings : ScriptableObject, IContentImport, IMenuItemRefreshable
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public ShooterSkinType type = default;
            public SkinSkeletonType skeletonType = default;
            public LevelTargetGenderType genderType = default;
            public SkinsColorsData[] skinsData = default;

            public string FindColorSkin(ShooterColorType colorType)
            {
                SkinsColorsData data = Array.Find(skinsData, e => e.key == colorType);
                return data == null ? string.Empty : data.skinName;
            }
        }


        [Serializable]
        public class LinkInfo
        {
            public SkinSkeletonType type = default;
            public ShooterSkinLink linkData = default;
            public SkeletonDataAsset asset = default;
        }


        [Serializable]
        public class VisualInfo
        {
            public ShooterSkinType type = default;

            public Sprite uiSprite = default;
            public Sprite uiOutlineSprite = default;
            public Sprite uiOutlineDisabledSprite = default;
        }

        [Serializable]
        public class ProgressReachInfo
        {
            public ShooterSkinType skinType = default;
        }


        [Serializable]
        public class FxData : BaseColorsData
        {
            public string idleFxKey = default;
        }

        #endregion



        #region Fields

        public LinkInfo[] linkInfo = default;

        public Data[] skinsData = default;
        public VisualInfo[] visualInfo = default;
        public ProgressReachInfo[] progressReachInfo = default;
        public ProgressReachInfo[] sequenceProgressReachInfo = default;

        [Header("Fxs")]
        public FxData[] fxData = default;

        #endregion



        #region Methods

        public ShooterSkinType[] FindSkinsTypeForProgress(int showIndex, IShopService shopService)
        {
            ShooterSkinType[] result = Array.Empty<ShooterSkinType>();

            if (showIndex < sequenceProgressReachInfo.Length &&
                !shopService.ShooterSkins.IsBought(sequenceProgressReachInfo[showIndex].skinType))
            {
                result = result.Add(sequenceProgressReachInfo[showIndex].skinType);
            }
            else
            {
                result = progressReachInfo
                            .Select(s => s.skinType)
                            .Where(e => !shopService.ShooterSkins.IsBought(e))
                            .ToArray();
            }

            return result;
        }


        public SkeletonDataAsset GetSkeletonDataAsset(ShooterSkinType type)
        {
            Data foundData = Array.Find(skinsData, e => e.type == type);

            SkinSkeletonType skeletonType = foundData == null ? SkinSkeletonType.None : foundData.skeletonType;

            LinkInfo foundInfo = Array.Find(linkInfo, e => e.type == skeletonType);
            return foundInfo == null ? null : foundInfo.asset;
        }


        public string GetAssetSkin(ShooterSkinType type, ShooterColorType colorType)
        {
            Data foundData = Array.Find(skinsData, e => e.type == type);
            return foundData == null ? default : foundData.FindColorSkin(colorType);
        }


        public Sprite GetSkinUiSprite(ShooterSkinType type)
        {
            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null ? default : foundInfo.uiSprite;
        }

        public Sprite GetUiOutlineSprite(ShooterSkinType type)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null || foundInfo.uiOutlineSprite == null ? fallbackSprite : foundInfo.uiOutlineSprite;
        }


        public Sprite GetUiOutlineDisabledSprite(ShooterSkinType type)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null || foundInfo.uiOutlineDisabledSprite == null ? fallbackSprite : foundInfo.uiOutlineDisabledSprite;
        }
        

        public SkinSkeletonType FindSkinSkeletonType(ShooterSkinType type)
        {
            Data foundData = Array.Find(skinsData, e => e.type == type);
            if (foundData == null)
            {
                CustomDebug.Log($"Could not find data. For ShooterSkinType: {type}");
            }
            
            return foundData == null ? SkinSkeletonType.None : foundData.skeletonType;
        }


        public SkinSkeletonType FindSkinSkeletonType(ShooterSkinLink currentSkinLink)
        {
            LinkInfo foundInfo = Array.Find(linkInfo, e => e.linkData == currentSkinLink);

            if (foundInfo == null)
            {
                CustomDebug.Log($"No found data for link: {currentSkinLink} in {this}.");
            }

            return foundInfo == default ? default : foundInfo.type;
        }


        public ShooterSkinLink GetSkinLink(ShooterSkinType type)
        {
            SkinSkeletonType skeletonType = FindSkinSkeletonType(type);
            ShooterSkinLink foundLink = GetSkinLink(skeletonType);

            return foundLink;
        }



        public LevelTargetGenderType GetSkinGenderType(ShooterSkinType type)
        {
            Data foundData = Array.Find(skinsData, e => e.type == type);
            return foundData == null ? default : foundData.genderType;
        }


        public string FindIdleFxsKey(ShooterColorType colorType)
        {
            FxData data = FindFxsData(colorType);
            return data == null ? default : data.idleFxKey;
        }


        private ShooterSkinLink GetSkinLink(SkinSkeletonType type)
        {
            LinkInfo foundInfo = Array.Find(linkInfo, e => e.type == type);
            return foundInfo == null ? null : foundInfo.linkData;
        }


        private FxData FindFxsData(ShooterColorType colorType)
        {
            FxData foundInfo = Array.Find(fxData, e => e.key == colorType);

            if (foundInfo == null)
            {
                CustomDebug.Log($"No data found for colorType {colorType} in {this}");
            }

            return foundInfo;
        }

        #endregion



        #region IContentImport

        public async void RefreshFromMenuItem()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 1566311688;

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
            const int DataGID = 1566311688;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (result) =>
            {
                visualInfo = Array.Empty<VisualInfo>();
                skinsData = Array.Empty<Data>();
                FillSkinsTypeEnum(result);
                ParseDataFromString(result);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            });
        }


        private void FillSkinsTypeEnum(string value)
        {
            const string SkinsTypesFilePath = "Assets/Scripts/GameFlow/Shop/Skins/ShooterSkin/ShooterSkinType.cs";
            const string WriteEnumFormat = "{0} = {1},";
            int enumIndex = default;

            if (File.Exists(SkinsTypesFilePath))
            {
                File.Delete(SkinsTypesFilePath);
            }

            StreamWriter outfile = new StreamWriter(SkinsTypesFilePath);

            outfile.WriteLine("namespace Drawmasters");
            outfile.WriteLine("{");
            outfile.WriteLine("    public enum ShooterSkinType");
            outfile.WriteLine("    {");
            outfile.WriteLine($" {string.Format(WriteEnumFormat, "None", enumIndex)}");
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


        private void ParseDataFromString(string value)
        {
            using (var reader = new StringReader(value))
            {
                string line = reader.ReadLine(); // skip header
                line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    string[] values = CSVParseUtility.ParseRowsToArray(line);

                    if (Enum.TryParse(values[0], out ShooterSkinType shooterSkinType))
                    {
                        Data dataToAdd = new Data();
                        dataToAdd.type = shooterSkinType;
                        dataToAdd.skeletonType = (SkinSkeletonType)Enum.Parse(typeof(SkinSkeletonType), values[1]);
                        dataToAdd.genderType = (LevelTargetGenderType)Enum.Parse(typeof(LevelTargetGenderType), values[3]);

                        SkinsColorsData[] skinsColorsData = Array.Empty<SkinsColorsData>();
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.None, skinName = values[4] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Red, skinName = values[4] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Blue, skinName = values[5] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Green, skinName = values[6] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Default, skinName = values[7] });

                        dataToAdd.skinsData = skinsColorsData;
                        skinsData = skinsData.Add(dataToAdd);

                        VisualInfo infoToAdd = new VisualInfo();
                        infoToAdd.type = shooterSkinType;

                        List<Sprite> allSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/Icons/Shooters");
                        infoToAdd.uiSprite = allSprites.Find(e => e.name.Equals(values[2], StringComparison.Ordinal));

                        List<Sprite> allUiOutlineSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/OutlineIcons/Shooters");
                        infoToAdd.uiOutlineSprite = allUiOutlineSprites.Find(e => e.name.Equals(values[8], StringComparison.Ordinal));
                        infoToAdd.uiOutlineDisabledSprite = allUiOutlineSprites.Find(e => e.name.Equals(values[9], StringComparison.Ordinal));

                        visualInfo = visualInfo.Add(infoToAdd);
                    }
                    else
                    {
                        Debug.Log($"<color=red>Parse error. Can't find shooter skin type {values[0]}" +
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
