using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileShooterSkinComponent : ProjectileSkinComponent
    {
        #region Properties

        protected override Sprite MainRendererSprite
        {
            get
            {
                WeaponSkinType weaponSkinType = currentWeaponType.ToWeaponSkinType();
                ProjectileSkinsSettings settings = IngameData.Settings.projectileSkinsSettings;

                return settings.GetProjectileSprite(weaponSkinType, mainProjectile.ColorType);
            }
        }

        #endregion




        #region Class lifecycle

        public ProjectileShooterSkinComponent(SpriteRenderer _mainRenderer) : base(_mainRenderer)
        {
        }

        #endregion 
    }
}
