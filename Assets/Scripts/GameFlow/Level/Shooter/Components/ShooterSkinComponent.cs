using System;
using Modules.Sound;
using Drawmasters.Effects;
using Drawmasters.Levels.Data;
using Spine;
using UnityEngine;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;
using Modules.General.InAppPurchase;
using Drawmasters.Statistics.Data;
using TransformMode = Drawmasters.Effects.TransformMode;


namespace Drawmasters.Levels
{
    public class ShooterSkinComponent : ShooterComponent
    {
        #region Fields

        public static event Action<WeaponSkinType> OnWeaponSkinApplied;

        private bool allowChangeSkins;

        private ILevelEnvironment levelEnvironment;

        private PlayerData playerData;

        #endregion



        #region Methods

        public override void StartGame()
        {
            playerData.OnShooterSkinSetted += OnSkinChanged;
            playerData.OnWeaponSkinSetted += OnSkinChanged;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            allowChangeSkins = true;
        }


        public override void Initialize(Shooter _shooter)
        {
            base.Initialize(_shooter);

            playerData = GameServices.Instance.PlayerStatisticService.PlayerData;
            levelEnvironment = GameServices.Instance.LevelEnvironment;
        }


        public override void Deinitialize()
        {
            playerData.OnShooterSkinSetted -= OnSkinChanged;
            playerData.OnWeaponSkinSetted -= OnSkinChanged;

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            allowChangeSkins = false;
        }


        private Skin FindSkin(string name)
        {
            if (name == null)
            {
                return null;
            }

            Skin result = shooter.SkeletonAnimation.Skeleton.Data.FindSkin(name);
            return result;
        }


        private void RefreshSkin()
        {
            if (!allowChangeSkins || shooter == null)
            {
                return;
            }

            bool isSubscriptionActive = SubscriptionManager.Instance.IsSubscriptionActive;
            
            if (!isSubscriptionActive)
            {
                ShooterSkinType currentShooterSkinType = playerData.CurrentSkin;
                bool isCurrentShooterSkinForSubscription = IngameData.Settings.subscriptionRewardSettings.IsSkinForSubscription(currentShooterSkinType);

                WeaponType currentWeaponType = levelEnvironment.Context.WeaponType;
                WeaponSkinType currentWeaponSkinType = playerData.GetCurrentWeaponSkin(currentWeaponType);
                bool isCurrentWeaponSkinForSubscription = IngameData.Settings.subscriptionRewardSettings.IsSkinForSubscription(currentWeaponSkinType);

                bool shouldResetSkins = isCurrentShooterSkinForSubscription || isCurrentWeaponSkinForSubscription;

                if (shouldResetSkins)
                {
                    playerData.ResetSkins();
                }

                IngameData.Settings.subscriptionRewardSettings.CancelSubscriptionReward();
            }
            
            ShooterColorType colorType = shooter.ColorType;
            WeaponType weaponType = levelEnvironment.Context.WeaponType;
            
            if (levelEnvironment.Context.SceneMode == GameMode.MenuScene)
            {
                weaponType = levelEnvironment.Context.WeaponType;
            }
            else if (levelEnvironment.Context.Mode.IsHitmastersLiveOps())
            {
                colorType = ShooterColorType.Default;
                GameModesInfo.TryConvertModeToWeapon(levelEnvironment.Context.Mode, out weaponType);
            }

            ShooterSkinType shooterSkinType = playerData.CurrentSkin;
            shooter.RefreshSkin(shooterSkinType);

            string shooterSkinName = IngameData.Settings.shooterSkinsSettings.GetAssetSkin(shooterSkinType, colorType);

            Skin shooterFoundSkin = FindSkin(shooterSkinName);
            
            WeaponSkinType weaponSkinType = playerData.GetCurrentWeaponSkin(weaponType);
            string weaponSkinName = IngameData.Settings.projectileSkinsSettings.GetAssetSkin(weaponSkinType, colorType);
            Skin weaponFoundSkin = FindSkin(weaponSkinName);

            if (shooterFoundSkin == null ||
                weaponFoundSkin == null)
            {
                CustomDebug.Log($"Can't set skin type {shooterSkinType} and weapon type {weaponSkinType} " +
                    $"\nfor {shooter.SkeletonAnimation.Skeleton} string {shooterSkinName}." +
                    $"\n Weapon: {weaponFoundSkin} // Shooter: {shooterFoundSkin} // Weapon: {weaponSkinName}");
                return;
            }

            Skin combined = new Skin("shooter_and_weapon_skins");
            combined.AddSkin(shooterFoundSkin);
            combined.AddSkin(weaponFoundSkin);

            shooter.SkeletonAnimation.Initialize(true);
            shooter.SkeletonAnimation.skeleton.SetSkin(combined);
            shooter.SkeletonAnimation.skeleton.SetToSetupPose();

            OnWeaponSkinApplied?.Invoke(weaponSkinType);
        }

        #endregion



        #region Events handlers

        private void OnSkinChanged()
        {
            RefreshSkin();

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.WEAPONSKINSWIPE);

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISkinsCharacterChange,
                                                  Vector3.zero,
                                                  Quaternion.identity,
                                                  shooter.transform,
                                                  TransformMode.Local);
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.Initialized)
            {
                RefreshSkin();
            }
        }

        #endregion
    }
}
