namespace Drawmasters.Levels
{
    public class ProjectileShooterTrail : ProjectileTrail
    {
        #region Methods

        protected override string GetTrailFxKey()
        {
            WeaponSkinType weaponSkinType = currentWeaponType.ToWeaponSkinType();
            string result = IngameData.Settings.projectileSkinsSettings.GetTrailEffectKey(weaponSkinType);

            return result;
        }

        #endregion
    }
}
