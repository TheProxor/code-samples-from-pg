namespace Drawmasters.Levels
{
    public class ProjectileRocketTrail : ProjectileTrail
    {
        #region Methods

        protected override string GetTrailFxKey() =>
            IngameData.Settings.bossLevelTargetSettings.FindTrailFxKey(mainProjectile.ColorType);
    
        #endregion
    }
}
