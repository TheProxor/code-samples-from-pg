using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ShooterSubscriptionIdleFxsComponent : ShooterIdleFxsAbstractComponent
    {
        #region Properties

        protected override bool ShouldEnable
        {
            get
            {
                LevelContext context = GameServices.Instance.LevelEnvironment.Context;
                bool isSceneMode = context.SceneMode.IsSceneMode();

                ShooterSkinType currentSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin;
                bool isSkinForSubscription = IngameData.Settings.subscriptionRewardSettings.IsSkinForSubscription(currentSkinType);

                return !isSceneMode && isSkinForSubscription;
            }
        }

        #endregion



        #region Methods

        protected override (string fxKey, string boneName)[] GetFxKeyAndBoneNames()
        {
            (string fxKey, string boneName)[] result = new (string fxKey, string boneName)[]
            {
                IngameData.Settings.subscriptionRewardSettings.FindFxsKeyAndBoneName(shooter.ColorType)
            };

            return result;
        }

        #endregion
    }
}
