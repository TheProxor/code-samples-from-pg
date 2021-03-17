using System;
using UnityEngine;
using Spine.Unity;
using Drawmasters.Utils;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "ProjectileSkinsSettings",
                 menuName = NamingUtility.MenuItems.IngameSettings + "ProjectileSkinsSettings")]
    public partial class ProjectileSkinsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            public WeaponSkinType[] types = default;

            [Header("Shot fx")]
            public string vfxBoneName = default;

            public string shotEffectKey = default;

            [Header("Trail")]
            public string trailName = default;

            [Header("Destroy fx")]
            public string effectKeyOnSmash = default;

            [Header("Sfx")]
            public string levelTargetCollisionSfx = default;
            public string projectilesBetweenCollisionSfx = default;
            public string[] soundEffectKeyOnDestroy = default;
            public string shotSoundEffectName = default;
            public string reloadSoundEffectName = default;

            [Header("Aim ray")]
            public float aimRayOffset = default;

            public SkinsColorsData[] skinsData = default;
            public ProjectileColorsData[] projectilesData = default;

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
        private class ProjectileColorsData : BaseColorsData
        {
            public Sprite projectileSprite = default;
        }

        #endregion



        #region Fields

        [Header("Common data")]
        [SerializeField] private Data[] data = default;

        #endregion



        #region Methods

        public Vector3 GetShotVfxPosition(SkeletonAnimation skeletonAnimation, WeaponSkinType type)
        {
            Data foundData = FindData(type);
            string boneName = foundData == null ? default : foundData.vfxBoneName;

            return SpineUtility.BoneToWorldPosition(boneName, skeletonAnimation);
        }


        public string GetShotEffectKey(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.shotEffectKey;
        }


        public float GetAimRayOffset(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? 0.0f : foundData.aimRayOffset;
        }


        public string GetTrailEffectKey(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.trailName;
        }


        public string GetReloadSfxKey(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.reloadSoundEffectName;
        }


        public string GetShotSoundEffectKey(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.shotSoundEffectName;
        }


        public string GetEffectOnSmashKey(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.effectKeyOnSmash;
        }


        public string GetLevelTargetCollisionSfx(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.levelTargetCollisionSfx;
        }


        public string GetProjectilesBetweenCollisionSfx(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? string.Empty : foundData.projectilesBetweenCollisionSfx;
        }
        

        public string[] GetDestroySoundEffectKeys(WeaponSkinType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? Array.Empty<string>() : foundData.soundEffectKeyOnDestroy;
        }


        public Sprite GetProjectileSprite(WeaponSkinType weaponSkinType, ShooterColorType colorType)
        {
            Data foundData = FindData(weaponSkinType);
            return foundData == null ? default : foundData.FindProjectileSprite(colorType);
        }


        public string GetAssetSkin(WeaponSkinType weaponSkinType, ShooterColorType colorType)
        {
            Data foundData = FindData(weaponSkinType);
            return foundData == null ? string.Empty : foundData.FindColorSkin(colorType);
        }


        private Data FindData(WeaponSkinType type)
        {
            Data result = Array.Find(data, e => Array.Exists(e.types, t => t == type));

            if (result == null)
            {
                CustomDebug.Log($"No data found for type {type} in {this}");
            }

            return result;
        }

        #endregion
    }
}
