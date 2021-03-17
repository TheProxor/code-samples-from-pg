using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileBossSkinComponent : ProjectileComponent
    {
        #region Fields

        private readonly SpriteRenderer mainRenderer;

        #endregion



        #region Class lifecycle

        public ProjectileBossSkinComponent(SpriteRenderer _mainRenderer)
        {
            mainRenderer = _mainRenderer;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            BossLevelTargetSettings settings = IngameData.Settings.bossLevelTargetSettings;
            mainRenderer.sprite = settings.FindVisualSprite(mainProjectile.ColorType);
        }

        #endregion
    }
}
