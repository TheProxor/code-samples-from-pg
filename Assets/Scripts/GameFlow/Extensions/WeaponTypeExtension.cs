using Drawmasters.ServiceUtil;

namespace Drawmasters
{
    public static class WeaponTypeExtension
    {
        #region Public methods

        public static WeaponSkinType ToWeaponSkinType (this WeaponType skinType)
            =>  GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(skinType);

        #endregion
    }
}
