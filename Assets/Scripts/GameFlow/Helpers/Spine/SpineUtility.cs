using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using System;
using Spine;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class SpineUtility
    {
        #region API

        public static GameObject InstantiateBoneFollower(SkeletonRenderer skeletonRenderer, string boneName, Transform parent)
        {
            string goName = string.Concat(boneName, "Fx");
            GameObject effectRoot = new GameObject(goName);
            effectRoot.transform.SetParent(parent);
            BoneFollower follower = effectRoot.AddComponent<BoneFollower>();
            follower.SkeletonRenderer = skeletonRenderer;
            follower.SetBone(boneName);

            return effectRoot;
        }


        public static GameObject InstantiateBoneFollower(SkeletonGraphic skeletonGraphic, string boneName, Transform parent)
        {
            string goName = string.Concat(boneName, "Fx");
            GameObject effectRoot = new GameObject(goName);
            effectRoot.AddComponent<RectTransform>();
            effectRoot.transform.SetParent(parent);
            BoneFollowerGraphic follower = effectRoot.AddComponent<BoneFollowerGraphic>();
            follower.SkeletonGraphic = skeletonGraphic;
            follower.SetBone(boneName);

            return effectRoot;
        }


        public static void SafeRefreshSkeleton(in SkeletonAnimation skeletonAnimation)
        {
            if (skeletonAnimation.AnimationState != null)
            {
                skeletonAnimation.AnimationState.ClearTracks();
                skeletonAnimation.AnimationState.Update(default);
            }

            if (skeletonAnimation.Skeleton != null)
            {
                skeletonAnimation.Skeleton.SetToSetupPose();
            }
        }


        public static TrackEntry SafeSetAnimation(in SkeletonAnimation skeletonAnimation, string animationName, int animationIndex = 0, bool loop = false, Action callback = default)
        {
            TrackEntry result = default;

            if (skeletonAnimation.AnimationState != null)
            {
                bool isAnimationExists = !string.IsNullOrEmpty(animationName) && skeletonAnimation.Skeleton.Data.FindAnimation(animationName) != null;
                if (isAnimationExists)
                {
                    result = skeletonAnimation.AnimationState.SetAnimation(animationIndex, animationName, loop);
                    AddCompleteCallback(ref result, callback);
                }
                else
                {
                    CustomDebug.Log($"No animation {animationName} for asset {skeletonAnimation}!");
                }
            }
            else
            {
                CustomDebug.Log($"Skeleton asset {skeletonAnimation} corrupted!");
            }

            return result;
        }


        public static void SafeAddAnimation(in SkeletonAnimation skeletonAnimation, string animationName, int animationIndex = 0, bool loop = false, float delay = default, Action callback = default)
        {
            if (skeletonAnimation.AnimationState != null)
            {
                bool isAnimationExists = !string.IsNullOrEmpty(animationName) && skeletonAnimation.Skeleton.Data.FindAnimation(animationName) != null;
                if (isAnimationExists)
                {
                    TrackEntry trackEntry = skeletonAnimation.AnimationState.AddAnimation(animationIndex, animationName, loop, delay);
                    AddCompleteCallback(ref trackEntry, callback);
                }
                else
                {
                    CustomDebug.Log($"No animation {animationName} for asset {skeletonAnimation}!");
                }
            }
            else
            {
                CustomDebug.Log($"Skeleton asset {skeletonAnimation} corrupted!");
            }
        }


        public static void SafeSetAnimation(in SkeletonGraphic skeletonGraphic, string animationName, int animationIndex = 0, bool loop = false, Action callback = default)
        {
            if (skeletonGraphic.AnimationState != null)
            {
                bool isAnimationExists = !string.IsNullOrEmpty(animationName) && skeletonGraphic.Skeleton.Data.FindAnimation(animationName) != null;
                if (isAnimationExists)
                {
                    TrackEntry trackEntry = skeletonGraphic.AnimationState.SetAnimation(animationIndex, animationName, loop);
                    AddCompleteCallback(ref trackEntry, callback);
                }
                else
                {
                    CustomDebug.Log($"No animation {animationName} for asset {skeletonGraphic}!");
                }
            }
            else
            {
                CustomDebug.Log($"Skeleton asset {skeletonGraphic} corrupted!");
            }
        }


        public static void SafeAddAnimation(in SkeletonGraphic skeletonGraphic, string animationName, int animationIndex = 0, bool loop = false, Action callback = default, float delay = default)
        {
            if (skeletonGraphic.AnimationState != null)
            {
                bool isAnimationExists = !string.IsNullOrEmpty(animationName) && skeletonGraphic.Skeleton.Data.FindAnimation(animationName) != null;
                if (isAnimationExists)
                {
                    TrackEntry trackEntry = skeletonGraphic.AnimationState.AddAnimation(animationIndex, animationName, loop, delay);
                    AddCompleteCallback(ref trackEntry, callback);
                }
                else
                {
                    CustomDebug.Log($"No animation {animationName} for asset {skeletonGraphic}!");
                }
            }
            else
            {
                CustomDebug.Log($"Skeleton asset {skeletonGraphic} corrupted!");
            }
        }


        public static void SetAttachment(in SkeletonAnimation skeletonAnimation, string slotName, string attachmentName)
        {
            Attachment attachment = skeletonAnimation.skeleton.GetAttachment(slotName, attachmentName);

            if (attachment != null)
            {
                skeletonAnimation.skeleton.SetAttachment(slotName, attachmentName);
            }
        }


        public static Vector3 BoneToWorldPosition(string boneName, SkeletonGraphic skeletonGraphic, Canvas canvas)
        {
            Bone bone = skeletonGraphic.Skeleton.FindBone(boneName);
            float scale = canvas.referencePixelsPerUnit;
            Vector3 targetWorldPosition = skeletonGraphic.transform.TransformPoint(new Vector3(bone.WorldX * scale, bone.WorldY * scale, 0f));

            return targetWorldPosition;
        }


        public static Vector3 BoneToWorldPosition(string boneName, SkeletonAnimation skeletonAnimation)
        {
            Bone foundBone = skeletonAnimation.Skeleton.FindBone(boneName);
            return BoneToWorldPosition(foundBone, skeletonAnimation.transform);
        }


        public static Vector3 BoneToWorldPosition(Bone bone, Transform skeletonAnimationTransform)
        {
            Vector3 foundBonePosition = bone == null ? default : new Vector3(bone.WorldX, bone.WorldY);
            return skeletonAnimationTransform.TransformPoint(foundBonePosition);
        }


        public static void InitializeSkeletonAnimation(in ISkeletonAnimation animation, bool overwrite)
        {
            if (animation is SkeletonAnimation skeletonAnimation)
            {
                skeletonAnimation.Initialize(overwrite);
            }
            else if (animation is SkeletonGraphic skeletonGraphic)
            {
                skeletonGraphic.Initialize(overwrite);
            }
        }


        public static void SetShooterSkin(ShooterSkinType skinType, in ISkeletonAnimation skeletonAnimation, ShooterColorType colorType = ShooterColorType.Default)
        {
            string shooterSkinName = IngameData.Settings.shooterSkinsSettings.GetAssetSkin(skinType, colorType);
            Skin shooterFoundSkin = FindSkin(shooterSkinName, skeletonAnimation);

            if (shooterFoundSkin == null)
            {
                CustomDebug.Log($"Cannot set {skinType} skin");

                return;
            }

            InitializeSkeletonAnimation(skeletonAnimation, true);
            skeletonAnimation.Skeleton.SetSkin(shooterFoundSkin);
            skeletonAnimation.Skeleton.SetToSetupPose();
        }


        public static void SetShooterSkins(WeaponType weaponType, in ISkeletonAnimation skeletonAnimation, ShooterColorType colorType = ShooterColorType.Default)
        {
            ShooterSkinType shooterSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin;
            string shooterSkinName = IngameData.Settings.shooterSkinsSettings.GetAssetSkin(shooterSkinType, colorType);
            Skin shooterFoundSkin = FindSkin(shooterSkinName, skeletonAnimation);

            WeaponSkinType weaponSkinType = weaponType.ToWeaponSkinType();
            string weaponSkinName = IngameData.Settings.projectileSkinsSettings.GetAssetSkin(weaponSkinType, colorType);
            Skin weaponFoundSkin = FindSkin(weaponSkinName, skeletonAnimation);

            if (shooterFoundSkin == null ||
                weaponFoundSkin == null)
            {
                CustomDebug.Log($"Can't set skins for {skeletonAnimation} string {shooterSkinName}. Weapon: {weaponFoundSkin} // Shooter: {shooterFoundSkin} string {weaponSkinName}");
                return;
            }

            Skin combined = new Skin("shooter_and_weapon_skins");
            combined.AddSkin(shooterFoundSkin);
            combined.AddSkin(weaponFoundSkin);

            InitializeSkeletonAnimation(skeletonAnimation, true);
            skeletonAnimation.Skeleton.SetSkin(combined);
            skeletonAnimation.Skeleton.SetToSetupPose();
        }


        public static Skin FindSkin(string name, in ISkeletonAnimation skeletonAnimation)
        {
            if (name == null)
            {
                return null;
            }

            Skin result = skeletonAnimation.Skeleton.Data.FindSkin(name);
            return result;
        }


        public static Skin FindSkin(string name, in SkeletonAnimation skeletonAnimation)
        {
            if (name == null)
            {
                return null;
            }

            Skin result = skeletonAnimation.Skeleton.Data.FindSkin(name);
            return result;
        }


        public static void AddCompleteCallback(ref TrackEntry trackEntry, Action callback = default)
        {
            if (callback != null)
            {
                Spine.AnimationState.TrackEntryDelegate trCallback = null;
                trCallback = (v) =>
                {
                    callback();
                    v.Complete -= trCallback;
                };

                trackEntry.Complete += trCallback;
            }
        }


        public static void AddEndCallback(ref TrackEntry trackEntry, Action callback = default)
        {
            if (callback != null)
            {
                Spine.AnimationState.TrackEntryDelegate trCallback = null;
                trCallback = (v) =>
                {
                    callback();
                    v.End -= trCallback;
                };

                trackEntry.End += trCallback;
            }
        }

        #endregion
    }
}
