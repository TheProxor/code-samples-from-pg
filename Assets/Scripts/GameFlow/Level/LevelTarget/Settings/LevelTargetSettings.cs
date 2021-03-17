using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName ="New Level Target Settings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LevelTargetSettings")]
    public class LevelTargetSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class ProjectileData
        {
            public ProjectileType projectileType = default;

            public float baseDamageForLimb = default;
            public float projectileCollisionImpuls = default;
        }


        [Serializable]
        public class LimbData
        {
            public LevelTargetLimbType limbType = default;
            public string rootBone = default;

            [Tooltip("Этот урон рассчитывается по другому множителю!")]
            public float damageToChopOffAllLimbs = default;

            public float damageToChopOff = default;
            public float damageToExplode = default;
            public float damageMultiplier = default;

            public PerfectType perfectTypeOnProjectileCollision = default;

            [Header("Effects")]
            [Enum(typeof(EffectKeys))]
            public string chopOffEffectOnBodyKey = default;

            [Enum(typeof(EffectKeys))]
            public string chopOffEffectOnLimbKey = default;

            [Enum(typeof(EffectKeys))]
            public string explodeLimbKey = default;

            [Enum(typeof(EffectKeys))]
            public string acidDropEffectKey = EffectKeys.FxAcidDestroedCharacterLimb;

            [Enum(typeof(EffectKeys))]
            public string laserDestroyedEffectKey = default;
        }


        [Serializable]
        public class LimbPartData
        {
            public string rootBone = default;

            public float impulsMultiplier = default;
        }

        #endregion



        #region Fields

        [Header("Enemy settings")]

        public float minDamageToDie = default;

        public ProjectileData[] projectilesData = default;

        [Header("Limbs Data")]
        public LimbData[] limbData = default;

        [Header("Limb parts Data")]
        public LimbPartData[] limbPartsData = default;

        [Header("Bones torque settings")]
        public List<BoneTorque> bonesTorque = default;

        [Header("Perfects")]
        public float hittedMinDamageForSkyFallPerfect = default;

        public float minDamageForSkyFallPerfect = default;
        public float hittedMinDamageForObjectDropPerfect = default;

        public string[] hittedBonesNamesForPerfects = default;

        #endregion



        #region Methods

        public ProjectileData FindProjectileData(ProjectileType type)
        {
            ProjectileData result = Array.Find(projectilesData, element => element.projectileType == type);

            if (result == null)
            {
                CustomDebug.Log($"No Limb Data found for type {type}");
            }

            return result;
        }


        public bool IsLimbDataExists(string bone) =>
            Array.Exists(limbData, element => element.rootBone == bone);


        public LimbData FindLimbData(string bone)
        {
            LimbData result = Array.Find(limbData, element => element.rootBone == bone);

            if (result == null)
            {
                CustomDebug.Log($"No Limb Data found for bone {bone}");
            }

            return result;
        }


        public LimbPartData FindLimbPartData(string bone)
        {
            LimbPartData result = Array.Find(limbPartsData, element => element.rootBone == bone);

            if (result == null)
            {
                CustomDebug.Log($"No Limb Part Data found for bone {bone}");
            }

            return result;
        }

        #endregion
    }
}
