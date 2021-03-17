using System.Collections.Generic;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ProjectilePetDestroyOnCollisionComponent : ProjectileDestroyOnCollisionComponent
    {
        #region Properties

        protected override string EffectKeyOnDestroy
        {
            get
            {
                PetWeaponSkinSettings petWeaponSkinSettings = IngameData.Settings.pets.weaponSkinSettings;
                PetSkinType petSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;
                string result = petWeaponSkinSettings.FindProjectileDestroyFxKey(petSkinType);
                return result;
            }
        }


        protected override string[] SoundsEffectKeyOnDestroy
        {
            get
            {
                // TODO: logic for each pet

                return new string[] { AudioKeys.Ingame.EXPLODE_1 };
            }
        }

        #endregion



        #region Class lifecycle

        public ProjectilePetDestroyOnCollisionComponent(List<CollidableObjectType> _typesThatDestroyProjectile,
            List<PhysicalLevelObjectType> _physicalLevelObjectTypes,
            CollisionNotifier _projectileCollisionNotifier) :
            base(_typesThatDestroyProjectile, _physicalLevelObjectTypes, _projectileCollisionNotifier)
        {
        }

        #endregion
    }
}
