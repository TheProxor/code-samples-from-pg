using System;
using Drawmasters.Levels;
using MoreMountains.NiceVibrations;
using Spine.Unity;
using UnityEngine;

namespace Drawmasters.Proposal
{
    public partial class ForceMeterUiSettings
    {
        #region Nested types

        [Serializable]
        private class ButtonData
        {
            public string animationTriggerKey = default;
            public string buttonlocalizationKey = default;
        }


        [Serializable]
        private class StageAnimationData
        {
            [Header("Forcemeter object animation")]
            public LampsStageAnimationData[] lampsStageAnimationData = default;

            [SpineBone(dataField = "forcemeterDataAsset")]
            public string rewardElementBone = default;

            public NumberAnimation trackTimeAnimation = default;

            [SpineAnimation(dataField = "forcemeterDataAsset")]
            public string forcemeterObjectName = default;
            [SpineAnimation(dataField = "forcemeterDataAsset")]
            public string elementAnimationName = default;

            [Header("Forcemeter object effects")]
            [Enum(typeof(EffectKeys))] public string buttonEffectKey = default;

            [Enum(typeof(EffectKeys))] public string shooterEffectKey = default;
            [Enum(typeof(AudioKeys.Ui))] public string shooterSfxKey = default;

            [Enum(typeof(EffectKeys))] public string rewardReachEffectKey = default;

            [Enum(typeof(AudioKeys.Ui))] public string maleSound = default;
            [Enum(typeof(AudioKeys.Ui))] public string femaleSound = default;

            [Header("Ui Forcemeter Screen animation")]
            public CameraShakeSettings.Shake shakeData = default;
            public ColorAnimation hitFadeAppearAnimation = default;
            public ColorAnimation hitFadeDisappearAnimation = default;

            public HapticTypes hitHapticType = default;
        }

        [Serializable]
        public class LampsStageAnimationData
        {
            [SpineBone(dataField = "forcemeterDataAsset")]
            public string[] forcemeterLightBones = default;
            public float enableDelay = default;
        }

        #endregion
    }
}