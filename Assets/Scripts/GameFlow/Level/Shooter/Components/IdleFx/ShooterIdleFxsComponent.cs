using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ShooterIdleFxsComponent : ShooterIdleFxsAbstractComponent
    {
        #region Properties

        protected override bool ShouldEnable
        {
            get
            {
                ShooterSkinType currentSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin;
                bool isAnyFxsForDataExists = IngameData.Settings.shooterSkinsFxsSettings.IsAnyDataExists(currentSkinType, shooter.ColorType);

                return isAnyFxsForDataExists;
            }
        }

        #endregion



        #region Methods

        protected override (string fxKey, string boneName)[] GetFxKeyAndBoneNames()
        {
            ShooterSkinType currentSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin;
            (string fxKey, string boneName)[] result = IngameData.Settings.shooterSkinsFxsSettings.FindFxsKeyAndBoneName(currentSkinType, shooter.ColorType);

            return result;
        }

        #endregion
    }
}
