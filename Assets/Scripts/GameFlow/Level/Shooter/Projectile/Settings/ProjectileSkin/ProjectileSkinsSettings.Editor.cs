using System;
using System.Collections.Generic;
using System.IO;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class ProjectileSkinsSettings : IContentImport, IMenuItemRefreshable
    {
        #region Editor methods


        [Sirenix.OdinInspector.Button]
        private void FindIndexType(WeaponSkinType weaponSkinType)
        {
            int index = Array.FindIndex(data, e => Array.Exists(e.types, t => t == weaponSkinType));
            Debug.Log($"Index for {weaponSkinType} is {index}");
        }


        [Sirenix.OdinInspector.Button]
        private void FindMissedTypes()
        {
            foreach (var wt in (WeaponSkinType[])Enum.GetValues(typeof(WeaponSkinType)))
            {
                bool exists = Array.Exists(data, e => Array.Exists(e.types, t => t == wt));

                if (!exists)
                {
                    Debug.Log($"{wt} not fount!");
                }
            }
        }


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

            StreamWriter outfile = new StreamWriter(SkinsTypesFilePath);

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
                        dataToAdd.types = new WeaponSkinType[] { weaponSkinType };

                        List<Sprite> allBulletSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ingame/Weapon");

                        ProjectileColorsData[] projectileColorsData = Array.Empty<ProjectileColorsData>();
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.None, projectileSprite = allBulletSprites.Find(e => e.name == values[14]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Red, projectileSprite = allBulletSprites.Find(e => e.name == values[14]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Blue, projectileSprite = allBulletSprites.Find(e => e.name == values[16]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Green, projectileSprite = allBulletSprites.Find(e => e.name == values[18]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Default, projectileSprite = allBulletSprites.Find(e => e.name == values[14]) });

                        dataToAdd.projectilesData = projectileColorsData;

                        dataToAdd.shotEffectKey = values[1];
                        dataToAdd.trailName = values[2];
                        dataToAdd.effectKeyOnSmash = values[3];
                        dataToAdd.shotSoundEffectName = values[4];
                        dataToAdd.levelTargetCollisionSfx = values[5];
                        dataToAdd.vfxBoneName = values[6];
                        dataToAdd.aimRayOffset = int.Parse(values[7]);
                        dataToAdd.projectilesBetweenCollisionSfx = values[11];

                        SkinsColorsData[] skinsColorsData = Array.Empty<SkinsColorsData>();
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.None, skinName = values[13] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Red, skinName = values[13] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Blue, skinName = values[15] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Green, skinName = values[17] });
                        skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Default, skinName = values[19] });

                        dataToAdd.skinsData = skinsColorsData;

                        data = data.Add(dataToAdd);
                    }
                    else
                    {
                        Debug.Log($"<color=red>Parse error. Can't find weaponskin type {values[0]} </color>");
                    }

                    line = reader.ReadLine();
                }
            }

            CustomDebug.Log($"<color=green> Reimorted data for {name} </color>");
        }

        #endregion
    }
}
