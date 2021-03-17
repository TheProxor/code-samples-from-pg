using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Sound;


namespace Drawmasters.Levels
{
    public class ReloadVfxComponent : ShooterComponent
    {
        #region Fields

        private string reloadSfxKey;

        #endregion



        #region Methods

        public override void StartGame()
        {
            WeaponType weaponType = GameServices.Instance.LevelEnvironment.Context.WeaponType;

            WeaponSkinType weaponSkinType = weaponType.ToWeaponSkinType();
            reloadSfxKey = IngameData.Settings.projectileSkinsSettings.GetReloadSfxKey(weaponSkinType);

            WeaponReload.OnWeaponReloadBegin += WeaponReload_OnWeaponReloadBegin;
        }


        public override void Deinitialize()
        {
            WeaponReload.OnWeaponReloadBegin -= WeaponReload_OnWeaponReloadBegin;
        }

        #endregion



        #region Events handlers

        private void WeaponReload_OnWeaponReloadBegin()
        {
            SoundManager.Instance.PlayOneShot(reloadSfxKey);
        }

        #endregion
    }
}