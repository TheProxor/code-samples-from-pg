using System.Collections.Generic;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class ProjectileShooterDestroyOnCollisionComponent : ProjectileDestroyOnCollisionComponent
    {
        #region Properties

        protected override string EffectKeyOnDestroy
        {
            get
            {
                WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(currentWeaponType);
                ProjectileSkinsSettings settings = IngameData.Settings.projectileSkinsSettings;

                return settings.GetEffectOnSmashKey(weaponSkinType);
            }
        }


        protected override string[] SoundsEffectKeyOnDestroy
        {
            get
            {
                WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(currentWeaponType);
                ProjectileSkinsSettings settings = IngameData.Settings.projectileSkinsSettings;

                return settings.GetDestroySoundEffectKeys(weaponSkinType);
            }
        }

        #endregion



        #region Class lifecycle

        public ProjectileShooterDestroyOnCollisionComponent(List<CollidableObjectType> _typesThatDestroyProjectile,
            List<PhysicalLevelObjectType> _physicalLevelObjectTypes,
            CollisionNotifier _projectileCollisionNotifier) :
            base(_typesThatDestroyProjectile, _physicalLevelObjectTypes, _projectileCollisionNotifier)
        {
        }

        #endregion
    }
}
