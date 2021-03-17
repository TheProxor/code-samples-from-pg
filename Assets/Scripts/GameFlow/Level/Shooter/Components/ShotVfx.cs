using Modules.Sound;
using Drawmasters.Effects;
using UnityEngine;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ShotVfx : ShooterComponent
    {
        #region Fields

        protected string shotEffectName;
        private string shotSoundEffectName;

        private EffectHandler effectHandler;

        private WeaponSkinType weaponSkinType;
        private ProjectileSkinsSettings settings;

        private ILevelEnvironment levelEnvironment;

        #endregion



        #region Properties

        protected Vector3 EffectPosition =>
            settings.GetShotVfxPosition(shooter.SkeletonAnimation, weaponSkinType);

        #endregion



        #region Methods

        public override void Initialize(Shooter _shooter)
        {
            base.Initialize(_shooter);

            levelEnvironment = GameServices.Instance.LevelEnvironment;

            WeaponType weaponType = levelEnvironment.Context.WeaponType;

            weaponSkinType = weaponType.ToWeaponSkinType();
            settings = IngameData.Settings.projectileSkinsSettings;

            shotEffectName = settings.GetShotEffectKey(weaponSkinType);
            shotSoundEffectName = settings.GetShotSoundEffectKey(weaponSkinType);

        }


        public override void StartGame()
        {
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            shooter.AddOnShootCallback(Shooter_OnShoot);
        }

        
        public override void Deinitialize()
        {
            if (effectHandler != null && !effectHandler.InPool)
            {
                EffectManager.Instance.PoolHelper.PushObject(effectHandler);
            }
            effectHandler = null;

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            shooter.RemoveOnShootCallback(Shooter_OnShoot);
        }


        protected virtual EffectHandler PlayFx(Vector2 direction)
        {
            if (!string.IsNullOrEmpty(shotEffectName))
            {
                Vector3 worldRotation = shooter.ProjectileEffectSpawnTransform.rotation.eulerAngles;

                if (shooter.LookingSide == ShooterLookSide.Left)
                {
                    worldRotation.z += 180.0f;
                }

                effectHandler = EffectManager.Instance.CreateSystem(shotEffectName,
                                                                    false,
                                                                    EffectPosition,
                                                                    Quaternion.Euler(worldRotation),
                                                                    shooter.ProjectileEffectSpawnTransform);
                if (effectHandler != null)
                {
                    effectHandler.Play(withClear: false);
                }
            }

            SoundManager.Instance.PlayOneShot(shotSoundEffectName);

            return effectHandler;
        }

        protected void ChangeSfx(string value) =>
            shotSoundEffectName = value;
        
        #endregion


        #region Events handlers

        private void Shooter_OnShoot(Vector2 direction) =>
            PlayFx(direction);
        

        private void Level_OnLevelStateChanged(LevelState state)
        {
            switch (state)
            {
                case LevelState.OutOfAmmo:

                    if (effectHandler != null && !effectHandler.InPool)
                    {
                        EffectManager.Instance.PoolHelper.PushObject(effectHandler);
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
