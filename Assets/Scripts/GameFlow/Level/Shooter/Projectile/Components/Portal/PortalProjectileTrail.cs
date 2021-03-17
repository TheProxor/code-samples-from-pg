using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public class PortalProjectileTrail : ProjectileTrail
    {
        #region Methods

        protected override string GetTrailFxKey()
        {
            WeaponSkinType type = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(WeaponType.HitmasteresPortalgun);

            return PortalController.createType == PortalObject.Type.First ? 
                EffectKeys.FxWeaponPortalGunBulletGreen : 
                EffectKeys.FxWeaponPortalGunBulletOrange;            
        }

        #endregion
    }
}
