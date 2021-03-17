using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "ShooterAnimationSettings",
                  menuName = NamingUtility.MenuItems.IngameSettings + "ShooterAnimationSettings")]
    public class ShooterAnimationSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class AimingData
        {
            public WeaponAnimationType[] aimingType = default;

            [SpineAnimation(dataField = "dataAsset")]
            public string hideWeaponAnimation = default;

            [SpineAnimation(dataField = "dataAsset")]
            public string tapReactionAnimation = default;

            [SpineAnimation(dataField = "dataAsset")]
            public string petTapReactionAnimation = default;
        }



        #endregion



        #region Fields

#pragma warning disable 0414

        // for reflection only
        [SerializeField] private SkeletonDataAsset dataAsset = default;

#pragma warning restore 0414

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] winNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] loseNames = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] danceAnimations = default;

        [SpineAnimation(dataField = "dataAsset")]
        public string forcemeterIdleName = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] forcemeterNames = default;

        [SpineEvent(dataField = "dataAsset")]
        public string hammerLightingEvent = default;

        [SpineAnimation(dataField = "dataAsset")]
        public string ragdollAnimationName = default;

        [SpineAnimation(dataField = "dataAsset")]
        public string reactionPetEnemyHit = default;

        [SpineAnimation(dataField = "dataAsset")]
        [SerializeField] private string[] fearAnimations = default;

        [SerializeField] private AimingData[] aimingData = default;

        public float emotionsAnimationsDelay = default;

        #endregion



        #region Properties

        public string RandomDanceAnimation =>
            danceAnimations.RandomObject();


        public string RandomWinName =>
            winNames.RandomObject();


        public string RandomLoseName =>
            loseNames.RandomObject();


        public string RandomFearAnimation =>
            fearAnimations.RandomObject();

        #endregion



        #region Methods

        public string GetForcemeterAnimation(int index)
        {
            string result = string.Empty;

            if (index >= 0 && index < forcemeterNames.Length)
            {
                result = forcemeterNames[index];
            }
            else
            {
                CustomDebug.Log($"No animation for index {index} in forcemeterNames");
            }

            return result;
        }

        #endregion



        #region Aimin Data

        public string FindHideWeaponAnimation(WeaponAnimationType weaponAnimationType)
        {
            AimingData foundData = FindAimingData(weaponAnimationType);
            return foundData == null ? default : foundData.hideWeaponAnimation;
        }


        public string FindTapReactionAnimation(WeaponAnimationType weaponAnimationType)
        {
            AimingData foundData = FindAimingData(weaponAnimationType);
            return foundData == null ? default : foundData.tapReactionAnimation;
        }


        public string FindPetTapReactionAnimation(WeaponAnimationType weaponAnimationType)
        {
            AimingData foundData = FindAimingData(weaponAnimationType);
            return foundData == null ? default : foundData.petTapReactionAnimation;
        }


        private AimingData FindAimingData(WeaponAnimationType weaponAnimationType)
        {
            AimingData foundData = Array.Find(aimingData, e => Array.Exists(e.aimingType, t => t == weaponAnimationType));
            if (foundData == null)
            {
                CustomDebug.Log($"No data found for weaponAnimationType {weaponAnimationType} in {this}");
            }
            return foundData;
        }

        #endregion
    }
}   
