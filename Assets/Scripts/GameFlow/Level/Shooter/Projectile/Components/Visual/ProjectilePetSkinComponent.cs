using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectilePetSkinComponent : ProjectileSkinComponent
    {
        #region Properties

        protected override Sprite MainRendererSprite
        {
            get
            {

                PetSkinType petSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;
                PetWeaponSkinSettings settings = IngameData.Settings.pets.weaponSkinSettings;

                return settings.FindProjectileSprite(petSkinType, mainProjectile.ColorType);
            }
        }

        #endregion




        #region Class lifecycle

        public ProjectilePetSkinComponent(SpriteRenderer _mainRenderer) : base(_mainRenderer)
        {
        }

        #endregion 
    }
}
