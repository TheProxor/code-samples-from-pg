using UnityEngine;
using Spine.Unity;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "ShooterAimingSettings",
                  menuName = NamingUtility.MenuItems.IngameSettings + "ShooterAimingSettings")]
    public class ShooterAimingSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            public ShooterAimingType aimingType = default;

            [SpineBone(dataField = "asset")]
            public string projectileSpawnBone = default;
        }


        [Serializable]
        private class AnimationData
        {
            [Space]
            public WeaponAnimationType weaponAimingType = default;

            [Space]
            [Header("Animations")]
            [SpineAnimation(dataField = "asset")]
            public string aimAnimation = default;

            [SpineAnimation(dataField = "asset")]
            public string shotAnimation = default;

            [SpineAnimation(dataField = "asset")]
            public string idleAnimation = default;

            [SpineAnimation(dataField = "asset")]
            public string[] greetingAnimations = default;
        }


        #endregion



        #region Fields

        #pragma warning disable 0414

        [Tooltip("only for reflection")]
        [SerializeField] private SkeletonDataAsset asset = default;

        #pragma warning restore

        [SerializeField] private Data[] data = default;
        [SerializeField] private AnimationData[] animationData = default;

        [SpineAnimation(dataField = "asset")]
        public string animationWithoutWeapon = default;

        [SpineEvent(dataField = "asset")]
        public string attachmentsEnableEvent = default;

        #endregion



        #region Methods

        public string GetShotAnimation(WeaponAnimationType type)
        {
            AnimationData foundData = FindAnimationData(type);
            return foundData == null ? default : foundData.shotAnimation;
        }


        public string GetIdleAnimation(WeaponAnimationType type)
        {
            AnimationData foundData = FindAnimationData(type);
            return foundData == null ? default : foundData.idleAnimation;
        }


        public string GetAimAnimation(WeaponAnimationType type)
        {
            AnimationData foundData = FindAnimationData(type);
            return foundData == null ? default : foundData.aimAnimation;
        }


        public string GetProjectileSpawnBone(ShooterAimingType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? default : foundData.projectileSpawnBone;
        }


        public string GetRandomGreetingAnimation(WeaponAnimationType type)
        {
            AnimationData animations = Array.Find(animationData, e => e.weaponAimingType == type);
            return animations == null ? string.Empty : animations.greetingAnimations.RandomObject();
        }

        public string GetAnimationWithoutWeapon() => animationWithoutWeapon;


        private AnimationData FindAnimationData(WeaponAnimationType type)
        {
            AnimationData result = Array.Find(animationData, e => e.weaponAimingType == type);

            if (result == null)
            {
                CustomDebug.Log($"No data found in {this} for aiming type {type}");
            }

            return result;
        }


        private Data FindData(ShooterAimingType type)
        {
            Data result = Array.Find(data, e => e.aimingType == type);

            if (result == null)
            {
                CustomDebug.Log($"No data found in {this} for aiming type {type}");
            }

            return result;
        }

        #endregion
    }
}
