using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Drawmasters.Levels;
using Drawmasters.Utils;
using Spine.Unity;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "PetSkinsSettings",
        menuName = NamingUtility.MenuItems.IngameSettings + "PetSkinsSettings")]
    public class PetSkinsSettings : ScriptableObject, IContentImport, IMenuItemRefreshable
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public PetSkinType type = default;
            public SkeletonDataAsset asset = default;
            public SkinsColorsData[] skinsData = default;

            [Header("Fxs")]
            public FxData[] fxData = default;

            public string FindColorSkin(ShooterColorType colorType)
            {
                SkinsColorsData data = Array.Find(skinsData, e => e.key == colorType);
                return data == null ? string.Empty : data.skinName;
            }
        }


        [Serializable]
        public class FxData : BaseColorsData
        {
            [Enum(typeof(EffectKeys))] public string idleFxKey = default;
            [Enum(typeof(EffectKeys))] public string appearanceFxKey = default;
            [Enum(typeof(EffectKeys))] public string disappearFxKey = default;
            [Enum(typeof(EffectKeys))] public string trailFxKey = default;
            [Enum(typeof(EffectKeys))] public string inceptionFxKey = default;
        }


        [Serializable]
        public class VisualInfo
        {
            public PetSkinType type = default;
            public Sprite uiSprite = default;
            public Sprite uiDisabledSprite = default;

            public Sprite uiOutlineSprite = default;
        }


        [Serializable]
        public class TransformsData
        {
            public PetSkinType type = default;

            public Vector3 targetPosition = default;
            public Vector3 targetLocalScale = default;
        }

        #endregion


        #region Fields

        [SerializeField] private Data[] skinsData = default;
        [SerializeField] private VisualInfo[] visualInfo = default;

        [Tooltip("Crunch for build release. Delete it in the next update")]
        [SerializeField] private TransformsData[] transformsData = default;
        [SerializeField] private TransformsData[] transformsDataBonusLevel = default;

        public PetSkinLink petLink = default;

        [Enum(typeof(EffectKeys))] public string sleepFxKey = default;

        #endregion


        #region Methods

        public Vector3 FindTargetPosition(PetSkinType type)
        {
            TransformsData foundData = Array.Find(transformsData, e => e.type == type);
            return foundData == null ? default : foundData.targetPosition;
        }


        public string GetAssetSkin(PetSkinType type, ShooterColorType colorType)
        {
            Data foundData = Array.Find(skinsData, e => e.type == type);
            return foundData == null ? default : foundData.FindColorSkin(colorType);
        }


        public SkeletonDataAsset GetSkeletonDataAsset(PetSkinType type)
        {
            Data foundData = Array.Find(skinsData, e => e.type == type);
            return foundData == null ? null : foundData.asset;
        }


        public Sprite GetSkinUiSprite(PetSkinType type)
        {
            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null ? default : foundInfo.uiSprite;
        }


        public Sprite GetSkinUiDisabledSprite(PetSkinType type)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null || foundInfo.uiDisabledSprite == null ? fallbackSprite : foundInfo.uiDisabledSprite;
        }


        public Sprite GetSkinUiOutlineSprite(PetSkinType type)
        {
            Sprite fallbackSprite = GetSkinUiSprite(type);

            VisualInfo foundInfo = Array.Find(visualInfo, e => e.type == type);
            return foundInfo == null || foundInfo.uiOutlineSprite == null ? fallbackSprite : foundInfo.uiOutlineSprite;
        }


        public string GetIdleFxsKey(PetSkinType type, ShooterColorType colorType)
        {
            FxData data = GetFxsData(type, colorType);
            return data == null ? default : data.idleFxKey;
        }


        public string GetAppearFxsKey(PetSkinType type, ShooterColorType colorType)
        {
            FxData data = GetFxsData(type, colorType);
            return data == null ? default : data.appearanceFxKey;
        }


        public string GetDisappearFxsKey(PetSkinType type, ShooterColorType colorType)
        {
            FxData data = GetFxsData(type, colorType);
            return data == null ? default : data.disappearFxKey;
        }


        public string GetTrailFxsKey(PetSkinType type, ShooterColorType colorType)
        {

            FxData data = GetFxsData(type, colorType);
            return data == null ? default : data.trailFxKey;
        }


        public string GetInceptionFxsKey(PetSkinType type, ShooterColorType colorType)
        {
            FxData data = GetFxsData(type, colorType);
            return data == null ? default : data.inceptionFxKey;
        }


        public Vector3 FindTargetLocalScaleLevel(PetSkinType type)
        {
            TransformsData foundData = Array.Find(transformsData, e => e.type == type);
            return foundData == null ? Vector3.one : foundData.targetLocalScale;
        }


        public Vector3 FindTargetLocalScaleBonusLevelAppear(PetSkinType type)
        {
            TransformsData foundData = Array.Find(transformsDataBonusLevel, e => e.type == type);
            return foundData == null ? Vector3.one : foundData.targetLocalScale;
        }


        private FxData GetFxsData(PetSkinType type, ShooterColorType colorType)
        {
            var foundData = skinsData.Find(x => x.type == type);
            if (foundData == null)
            {
                return null;
            }

            FxData foundInfo = Array.Find(foundData.fxData, e => e.key == colorType);
            if (foundInfo == null)
            {
                CustomDebug.Log($"No data found for colorType {colorType} in {this}");
            }

            return foundInfo;
        }

        #endregion




        #region IMenuItemRefreshable

        public async void RefreshFromMenuItem()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 977083876;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (result) =>
            {
                FillSkinsTypeEnum(result);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.Refresh();
#endif
            });
        }

        #endregion



        #region IContentImport

        public async void ReimportContent()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 977083876;

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
            const string SkinsTypesFilePath = "Assets/Scripts/GameFlow/Shop/Skins/Pet/PetSkinType.cs";
            const string WriteEnumFormat = "{0} = {1},";
            int enumIndex = default;

            if (File.Exists(SkinsTypesFilePath))
            {
                File.Delete(SkinsTypesFilePath);
            }

            StreamWriter outfile = new StreamWriter(SkinsTypesFilePath);

            outfile.WriteLine("namespace Drawmasters");
            outfile.WriteLine("{");
            outfile.WriteLine("    public enum PetSkinType");
            outfile.WriteLine("    {");

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

                    if (Enum.TryParse(values[0], out PetSkinType skinType))
                    {
                        Data dataToAdd = new Data();
                        dataToAdd.type = skinType;

                        SkinsColorsData[] skinsColorsData = Array.Empty<SkinsColorsData>();
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.None, skinName = values[5] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Red, skinName = values[2] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Blue, skinName = values[3] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Green, skinName = values[4] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Default, skinName = values[5] });

                        dataToAdd.skinsData = skinsColorsData;

                        const string allSkeletonDataAssetsPath = "Assets/Animation/SpineAnimation/Pets";
                        SkeletonDataAsset[] loadedAssets = ResourcesUtility.LoadAssetsAtPath<SkeletonDataAsset>(allSkeletonDataAssetsPath).ToArray();

                        var animationAsset = loadedAssets.Find(e => e.name == values[6]);
                        dataToAdd.asset = animationAsset;

                        skinsData = skinsData.Add(dataToAdd);

                        FxData[] fxData = Array.Empty<FxData>();
                        fxData = fxData.Add(new FxData { key = ShooterColorType.Red, appearanceFxKey = values[7], disappearFxKey = values[11], trailFxKey = values[15], idleFxKey = values[19], inceptionFxKey = values[23] });
                        fxData = fxData.Add(new FxData { key = ShooterColorType.Blue, appearanceFxKey = values[8], disappearFxKey = values[12], trailFxKey = values[16], idleFxKey = values[20], inceptionFxKey = values[24] });
                        fxData = fxData.Add(new FxData { key = ShooterColorType.Green, appearanceFxKey = values[9], disappearFxKey = values[13], trailFxKey = values[17], idleFxKey = values[21], inceptionFxKey = values[25] });

                        fxData = fxData.Add(new FxData { key = ShooterColorType.None, appearanceFxKey = values[10], disappearFxKey = values[14], trailFxKey = values[18], idleFxKey = values[22], inceptionFxKey = values[26] });
                        fxData = fxData.Add(new FxData { key = ShooterColorType.Default, appearanceFxKey = values[10], disappearFxKey = values[14], trailFxKey = values[18], idleFxKey = values[22], inceptionFxKey = values[26] });

                        dataToAdd.fxData = fxData;

                        VisualInfo infoToAdd = new VisualInfo();
                        infoToAdd.type = skinType;

                        List<Sprite> allSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/Icons/Pets");
                        infoToAdd.uiSprite = allSprites.Find(e => e.name.Equals(values[1], StringComparison.Ordinal));

                        List<Sprite> allUiOutlineSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/OutlineIcons/Pets");
                        infoToAdd.uiOutlineSprite = allUiOutlineSprites.Find(e => e.name.Equals(values[27], StringComparison.Ordinal));

                        List<Sprite> allDisabledSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ui/Icons/Pets/Disabled");
                        infoToAdd.uiDisabledSprite = allSprites.Find(e => e.name.Equals(values[28], StringComparison.Ordinal));

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