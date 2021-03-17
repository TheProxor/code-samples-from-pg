using System;
using System.Collections.Generic;
using System.IO;
using Drawmasters.Levels;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = nameof(PetWeaponSkinSettings),
                   menuName = NamingUtility.MenuItems.IngameSettings + nameof(PetWeaponSkinSettings))]
    public class PetWeaponSkinSettings : ScriptableObjectData<PetWeaponSkinSettings.Data, PetSkinType>, IContentImport
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<PetSkinType>
        {
            [Header("Trail")]
            public string trailName = default;
            public string projectileTrailName = default;
            
            [Header("Attack")]
            public string petAttackFxKey = default;
            public string projectileDestroyFxKey = default;

            [Header("Data")]
            public SkinsColorsData[] skinsData = default;
            public ProjectileColorsData[] projectilesData = default;

            [Header("Invoke")]
            public string petInvokeFxKey = default;

            public string FindColorSkin(ShooterColorType colorType)
            {
                SkinsColorsData data = Array.Find(skinsData, e => e.key == colorType);
                return data == null ? string.Empty : data.skinName;
            }


            public Sprite FindProjectileSprite(ShooterColorType colorType)
            {
                ProjectileColorsData data = Array.Find(projectilesData, e => e.key == colorType);

                if (data == null)
                {
                    CustomDebug.Log($"Cannot find sprite. Color type: {colorType}");
                }

                return data == null ? default : data.projectileSprite;
            }
        }


        [Serializable]
        public class ProjectileColorsData : BaseColorsData
        {
            public Sprite projectileSprite = default;
        }

        #endregion




        #region Methods

        public string FindPetAttackFxKey(PetSkinType skinType)
        {
            Data foundData = FindData(skinType);
            return foundData == null ? default : foundData.petAttackFxKey;
        }

        public string FindProjectileDestroyFxKey(PetSkinType skinType)
        {
            Data foundData = FindData(skinType);
            return foundData == null ? default : foundData.projectileDestroyFxKey;
        }


        public Sprite FindProjectileSprite(PetSkinType skinType, ShooterColorType colorType)
        {
            Data foundData = FindData(skinType);
            return foundData == null ? default : foundData.FindProjectileSprite(colorType);
        }


        public string FindPetTrailFxsKey(PetSkinType skinType)
        {
            Data foundData = FindData(skinType);
            return foundData == null ? default : foundData.trailName;
        }


        public string FindProjectileTrailFxsKey(PetSkinType skinType)
        {
            Data foundData = FindData(skinType);
            return foundData == null ? default : foundData.projectileTrailName;
        }


        public string FindPetInvokeFxKey(PetSkinType skinType)
        {
            Data foundData = FindData(skinType);
            return foundData == null ? default : foundData.petInvokeFxKey;
        }

        #endregion



        #region IContentImport

        public async void ReimportContent()
        {
            const string DataGoogleSheetID = "1XZgHEx6qPDwtxNP1bBAbZ1wXSog2TxgyFKCoGT1k7I4";
            const int DataGID = 1066498935;

            await CSVDownloader.ReadDataAsync(DataGoogleSheetID, DataGID, (result) =>
            {
                data = Array.Empty<Data>();
                ParseDataFromString(result);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            });
        }


        private void ParseDataFromString(string value)
        {
            Debug.Log(value);
            using (var reader = new StringReader(value))
            {
                reader.ReadLine(); // skip header 1
                reader.ReadLine(); // skip header 2

                string  line = reader.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    string[] values = CSVParseUtility.ParseRowsToArray(line);

                    if (Enum.TryParse(values[0], out PetSkinType weaponSkinType))
                    {
                        Data dataToAdd = new Data();
                        dataToAdd.key = weaponSkinType;

                        List<Sprite> allBulletSprites = ResourcesUtility.LoadAssetsAtPath<Sprite>("Assets/Textures/Ingame/PetWeapon");

                        ProjectileColorsData[] projectileColorsData = Array.Empty<ProjectileColorsData>();
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.None, projectileSprite = allBulletSprites.Find(e => e.name == values[1]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Default, projectileSprite = allBulletSprites.Find(e => e.name == values[1]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Red, projectileSprite = allBulletSprites.Find(e => e.name == values[1]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Blue, projectileSprite = allBulletSprites.Find(e => e.name == values[2]) });
                        projectileColorsData = projectileColorsData.Add(new ProjectileColorsData { key = ShooterColorType.Green, projectileSprite = allBulletSprites.Find(e => e.name == values[3]) });

                        dataToAdd.projectilesData = projectileColorsData;

                        dataToAdd.trailName = values[4];
                        dataToAdd.projectileTrailName = values[5];
                        dataToAdd.petAttackFxKey = values[6];
                        dataToAdd.projectileDestroyFxKey = values[7];
                        dataToAdd.petInvokeFxKey = values[8];

                        SkinsColorsData[] skinsColorsData = Array.Empty<SkinsColorsData>();
                        //skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.None, skinName = values[13] });
                        //skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Red, skinName = values[13] });
                        //skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Blue, skinName = values[15] });
                        //skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Green, skinName = values[17] });
                        //skinsColorsData = skinsColorsData.Add(new SkinsColorsData { key = ShooterColorType.Default, skinName = values[19] });

                        dataToAdd.skinsData = skinsColorsData;

                        data = data.Add(dataToAdd);
                    }
                    else
                    {
                        Debug.Log($"<color=red>Parse error. Can't find weapon skin type {values[0]} </color>");
                    }

                    line = reader.ReadLine();
                }
            }

            CustomDebug.Log($"<color=green> Reimorted data for {name} </color>");
        }

        #endregion
    }
}
