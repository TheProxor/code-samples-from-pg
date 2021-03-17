namespace Drawmasters.Levels
{
    public abstract class ProjectileSettingsComponent : ProjectileComponent
    {
        #region Public methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            WeaponSettings projectileSettings = IngameData.Settings.modesInfo.GetSettings(mode);
            ApplySettings(projectileSettings);
        }

        #endregion



        #region Protected methods

        protected abstract void ApplySettings(WeaponSettings settings);

        protected void LogError(string actualTypeName, string missingTypeName)
            => CustomDebug.Log($"{actualTypeName} doesn't implement {missingTypeName}.");

        #endregion
    }
}
