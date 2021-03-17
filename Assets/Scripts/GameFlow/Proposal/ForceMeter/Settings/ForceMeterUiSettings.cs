using System;
using MoreMountains.NiceVibrations;
using Drawmasters.Levels;
using Drawmasters.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "ForceMeterUiSettings",
                  menuName = NamingUtility.MenuItems.GuiSettings + "ForceMeterUiSettings")]
    public partial class ForceMeterUiSettings : ScriptableObject
    {
        #region Fields

        public FactorAnimation buttonsBlendAnimation;
        public float skinRewardOpenDelay = default;

        [SerializeField] private ButtonData[] buttonData = default;

        [Header("Animations")]

#pragma warning disable 0414

        // for reflection only
        [SerializeField] private SkeletonDataAsset forcemeterDataAsset = default;

#pragma warning restore 0414

        [SpineAnimation(dataField = "forcemeterDataAsset")]
        public string forcemeterFillProgressName = default;

        [Enum(typeof(EffectKeys))] public string lightEffectKeyName = default;

        [SpineBone(dataField = "forcemeterDataAsset")]
        [SerializeField] private string buttonRootBone = default;

        [SpineEvent(dataField = "forcemeterDataAsset")]
        public string forcemeterFillProgressEvent = default;

        [SerializeField] private StageAnimationData[] stageAnimationData = default;
        public int lampFxDisableIterationIndex = default;

        [Header("Power up data")]
        [Enum(typeof(EffectKeys))] public string lightningEffectKey = default;
        [Enum(typeof(EffectKeys))] public string hammerTrailKey = default;

        #endregion



        #region Properties

        public int StageAnimationDataLength =>
            stageAnimationData.Length;

        #endregion



        #region Methods

        public Vector3 GetButtonFxWorldPosition(SkeletonAnimation skeletonAnimation) =>
            SpineUtility.BoneToWorldPosition(buttonRootBone, skeletonAnimation);

        
        public string FindElementAnimation(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.elementAnimationName;
        }

        public LampsStageAnimationData[] FindForcemeterLampsData(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.lampsStageAnimationData;
        }


        public NumberAnimation FindTrackTimeAnimation(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.trackTimeAnimation;
        }


        public string FindForcemeterObjectName(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData == null ? string.Empty : foundData.forcemeterObjectName;
        }


        public string FindButtonEffectKey(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData == null ? string.Empty : foundData.buttonEffectKey;
        }


        public string FindShooterEffectKey(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            return foundData == null ? string.Empty : foundData.shooterEffectKey;
        }


        public string FindShooterSfxKey(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            return foundData == null ? string.Empty : foundData.shooterSfxKey;
        }


        public string FindRewardReachEffectKey(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            return foundData == null ? string.Empty : foundData.rewardReachEffectKey;
        }
        

        public string FindMaleSfxKey(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            return foundData == null ? string.Empty : foundData.maleSound;
        }
        

        public string FindFemaleSfxKey(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData == null ? string.Empty : foundData.femaleSound;
        }


        public Vector3 FindRewardElementWorldPosition(int index, SkeletonAnimation skeletonAnimation)
        {
            StageAnimationData foundData = FindStageData(index);
            
            string boneName = foundData?.rewardElementBone;

            Bone buttonBone = skeletonAnimation.skeleton.FindBone(boneName);

            Vector3 buttonSpineWorldPosition = 
                buttonBone == null ? 
                    default : 
                    new Vector3(buttonBone.WorldX, buttonBone.WorldY);
            
            return skeletonAnimation.transform.TransformPoint(buttonSpineWorldPosition);
        }


        public string FindButtonAnimationTriggerKey(int index)
        {
            ButtonData foundData = FindButtonData(index);
            
            return foundData == null ? string.Empty : foundData.animationTriggerKey;
        }


        public string FindButtonKey(int index)
        {
            ButtonData foundData = FindButtonData(index);
            
            return foundData == null ? string.Empty : foundData.buttonlocalizationKey;
        }


        public CameraShakeSettings.Shake FindShakeAnimation(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.shakeData;
        }


        public ColorAnimation FindHitFadeAppearAnimation(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.hitFadeAppearAnimation;
        }


        public HapticTypes FindHapticType(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.hitHapticType ?? default;
        }


        public ColorAnimation FindHitFadeDisappearAnimation(int index)
        {
            StageAnimationData foundData = FindStageData(index);
            
            return foundData?.hitFadeDisappearAnimation;
        }


        private ButtonData FindButtonData(int index)
        {
            ButtonData result = default;

            if (index >= 0 && index < buttonData.Length)
            {
                result = buttonData[index];
            }
            else
            {
                CustomDebug.Log($"No button data found for index {index}");
            }

            return result;
        }


        private StageAnimationData FindStageData(int index)
        {
            StageAnimationData result = default;

            if (index >= 0 && index < stageAnimationData.Length)
            {
                result = stageAnimationData[index];
            }
            else
            {
                CustomDebug.Log($"No button data found for index {index}");
            }

            return result;
        }

        #endregion
    }
}
