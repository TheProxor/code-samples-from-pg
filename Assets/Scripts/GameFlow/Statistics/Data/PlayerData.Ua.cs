using Drawmasters.Ua;


namespace Drawmasters.Statistics.Data
{
    public partial class PlayerData
    {
        #region Fields

        public static bool isUaBloodEnabled = !BuildInfo.IsUaBuild;
        
        public static bool IsUaKillingShootersEnabled
        {
            get => CustomPlayerPrefs.GetBool("ua_killing_shooters_enabled");
            set => CustomPlayerPrefs.SetBool("ua_killing_shooters_enabled", value);
        }
        
        #endregion
    }
}
