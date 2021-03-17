using UnityEngine;



namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "BossLauncherSettings",
                   menuName = NamingUtility.MenuItems.IngameSettings + "ModeInfo/BossLauncherSettings")]
    public class BossLauncherSettings : SniperSettings
    {
        #region Fields

        public override ProjectileType ProjectileType => ProjectileType.BossRocket;

        #endregion
    }
}
