using Spine;
using System;
using Spine.Unity;
using UnityEngine;
using Drawmasters.ServiceUtil;
using Drawmasters.Levels.Data;


namespace Drawmasters.Levels
{
    public class ShooterAimingAnimation : MonoBehaviour, IDeinitializable
    {
        #region Fields

        public event Action<ShooterLookSide> OnShouldTryFlip;
        public event Action OnRootsChanged;

        private const int AnimationIndex = 0;
        private const int ShotIndex = 1;

        [SerializeField] private BoneFollower projectileSpawnRoot = default;
        [SerializeField] private Transform projectileEffectSpawnRoot = default;
        [SerializeField] private CustomBoneFollower projectileEffectBoneFollower = default;
        [SerializeField] private Transform weaponRoot = default;
        [SerializeField] private Transform projectilesSpawnThrow = default;

        private string aimAnimation;
        private string shotAnimation;
        private string idleAnimation;

        private TrackEntry tracker;
        private TrackEntry shotTracker;
        private float totalAnimationDuration;

        private Shooter shooter;

        #endregion



        #region Properties

        public Transform ProjectileSpawnTransform => projectileSpawnRoot.transform;


        public CustomBoneFollower ProjectileEffectBoneFollower => projectileEffectBoneFollower;


        public Transform ProjectileEffectSpawnTransform => projectileEffectSpawnRoot;


        public Transform WeaponRoot => weaponRoot;
        
        public Transform ProjectilesSpawnThrow => projectilesSpawnThrow;

        #endregion



        #region Methods

        public void Initialize()
        {
            ShooterSkinComponent.OnWeaponSkinApplied += RefreshAimingData;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        public void Deinitialize()
        {
            ShooterSkinComponent.OnWeaponSkinApplied -= RefreshAimingData;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            if (shooter != null)
            {
                shooter.SkeletonAnimation.AnimationState.ClearTrack(AnimationIndex);
                shooter.SkeletonAnimation.AnimationState.ClearTrack(ShotIndex);
            }
        }


        public void SetupShooter(Shooter _shooter)
        {
            shooter = _shooter;

            WeaponType weaponType = GameServices.Instance.LevelEnvironment.Context.WeaponType;
            WeaponSkinType weaponSkinType = weaponType.ToWeaponSkinType();
            RefreshAimingData(weaponSkinType);
        }


        public void SetAnimation()
        {
            shooter.SkeletonAnimation.AnimationState.ClearTracks();
            shooter.SkeletonAnimation.skeleton.SetToSetupPose();

            Spine.Animation aimAnimationFound = string.IsNullOrEmpty(aimAnimation) ? null : shooter.SkeletonAnimation.Skeleton.Data.FindAnimation(aimAnimation);

            if (aimAnimationFound == null)
            {
                CustomDebug.Log($"No aim animation found. Animation name = {aimAnimation}");
                return;
            }

            tracker = shooter.SkeletonAnimation.AnimationState.SetAnimation(AnimationIndex, aimAnimation, true);
            shooter.SkeletonAnimation.skeleton.Update(0.0f);

            tracker.TimeScale = 0.0f;
        }
        

        public void ResetAnimation()
        {
            shooter.SkeletonAnimation.AnimationState.ClearTrack(AnimationIndex);

            if (tracker != null)
            {
                tracker.TimeScale = 1.0f;
            }
        }


        public void Aim(Vector2 aimDirection)
        {
            float angle = Mathf.Atan2(-aimDirection.x, aimDirection.y) * Mathf.Rad2Deg;

            weaponRoot.localEulerAngles = Vector3.forward * angle;

            ShooterLookSide targetLookingSide = (angle > 0.0f) ? ShooterLookSide.Left : ShooterLookSide.Right;

            if (tracker != null)
            {
                float time = Mathf.Abs(angle) / 180.0f * totalAnimationDuration;
                tracker.TrackTime = time;
            }

            shooter.SkeletonAnimation.AnimationState.Update(0f);

            OnShouldTryFlip?.Invoke(targetLookingSide);
        }


        public Transform GetAimStartPosition(WeaponType type)
        {
            Transform result = default;

            switch (type)
            {
                case WeaponType.Sniper:
                case WeaponType.HitmastersSniper:
                case WeaponType.HitmastersShotgun:
                case WeaponType.HitmastersGravitygun:
                case WeaponType.HitmasteresPortalgun:
                    result = WeaponRoot;
                    break;

                default:
                    CustomDebug.Log($"No logic for type {type} in {this}");
                    break;
            }

            return result;
        }


        public void SetShotAnimation()
        {
            shotTracker = shooter.SkeletonAnimation.AnimationState.SetAnimation(ShotIndex, shotAnimation, false);
            shotTracker.Complete += ShotTracker_Complete;
            shotTracker.TimeScale = 1.0f;
        }


        private void ShotTracker_Complete(TrackEntry trackEntry) =>
            shooter.SkeletonAnimation.AnimationState.ClearTrack(ShotIndex);

        #endregion



        #region Events handlers

        private void RefreshAimingData(WeaponSkinType weaponSkinType)
        {
            // A lot of null checks to support legacy mode (editor only) with legacy skeleton data assets.
            WeaponSkinSettings weaponSkinSettings = IngameData.Settings.weaponSkinSettings;
            ShooterAimingSettings aimingSettings = IngameData.Settings.shooterAimingSettings;

            WeaponAnimationType animationType = weaponSkinSettings.GetAnimationType(weaponSkinType);
            aimAnimation = aimingSettings.GetAimAnimation(animationType);
            shotAnimation = aimingSettings.GetShotAnimation(animationType);

            LevelContext levelContext = GameServices.Instance.LevelEnvironment.Context;
            bool isSceneMode = levelContext.SceneMode.IsSceneMode();
            bool isExcludedMode = shooter.CurrentGameMode.IsExcludedFromLoad();
            bool isModeOpen = levelContext.Mode.IsModeOpen();
            bool shouldHideWeapon = isSceneMode && (isExcludedMode || !isModeOpen);

            projectileSpawnRoot.SkeletonRenderer = shooter.SkeletonAnimation;
            if (shooter.SkeletonAnimation.Skeleton != null)
            {
                ShooterAimingType currentAimType = weaponSkinSettings.GetAimingType(weaponSkinType);
                string bone = aimingSettings.GetProjectileSpawnBone(currentAimType);
                if (!string.IsNullOrEmpty(bone) &&
                    projectileSpawnRoot.SkeletonRenderer.skeleton.FindBone(bone) != null)
                {
                    projectileSpawnRoot.SetBone(bone);
                }
                else
                {
                    Debug.Log($"No bone {bone} found");
                }

                idleAnimation = shouldHideWeapon ? aimingSettings.GetAnimationWithoutWeapon() : aimingSettings.GetIdleAnimation(animationType);
                Spine.Animation idleAnimationFound = string.IsNullOrEmpty(idleAnimation) ? null : shooter.SkeletonAnimation.Skeleton.Data.FindAnimation(idleAnimation);
                if (idleAnimationFound != null)
                {
                    shooter.SkeletonAnimation.AnimationState.AddAnimation(AnimationIndex, idleAnimation, true, 0f);
                }
            }
            else
            {
                CustomDebug.Log("Temp log. Maybe bug");
                Debug.Break();
            }

            totalAnimationDuration = string.IsNullOrEmpty(aimAnimation) ? default : shooter.SkeletonAnimation.skeleton.Data.FindAnimation(aimAnimation).Duration;

            //shooter.SkeletonAnimation.AnimationState.Update(0f);

            //TODO check many invokes

            OnRootsChanged?.Invoke();
        }


        private void Level_OnLevelStateChanged(LevelState obj)
        {
            if (obj == LevelState.AllTargetsHitted ||
                obj == LevelState.FriendlyDeath ||
                obj == LevelState.OutOfAmmo)
            {
                shooter.SkeletonAnimation.AnimationState.ClearTrack(ShotIndex);
            }
        }

        #endregion
    }
}
