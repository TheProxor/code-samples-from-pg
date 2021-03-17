using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileSfxCollisionComponent : ProjectileComponent
    {
        #region Fields

        private readonly CollisionNotifier collisionNotifier = default;
        private readonly List<CollidableObjectType> typesToRebound = default;
        private readonly IPlayerStatisticService playerStatistic;

        #endregion



        #region Lifecycle

        public ProjectileSfxCollisionComponent(CollisionNotifier _collisionNotifier, List<CollidableObjectType> _typesToRebound)
        {
            collisionNotifier = _collisionNotifier;
            typesToRebound = _typesToRebound;
            playerStatistic = GameServices.Instance.PlayerStatisticService;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType mode)
        {
            base.Initialize(mainProjectileValue, mode);

            collisionNotifier.OnCustomCollisionEnter2D += CollisionNotifier_OnCustomCollisionEnter2D;
        }


        public override void Deinitialize()
        {
            collisionNotifier.OnCustomCollisionEnter2D -= CollisionNotifier_OnCustomCollisionEnter2D;

            base.Deinitialize();
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomCollisionEnter2D(GameObject reference, Collision2D collision)
        {
            CollidableObject collidedObject = collision.gameObject.GetComponent<CollidableObject>();
            if (collidedObject != null)
            {
                if (typesToRebound.Contains(collidedObject.Type))
                {
                    SoundManager.Instance.PlaySound(SoundGroupKeys.RandomRicochetKey);
                }

                if (collidedObject.LevelTarget != null)
                {
                    WeaponSkinType weaponSkinType = playerStatistic.PlayerData.GetCurrentWeaponSkin(currentWeaponType);
                    ProjectileSkinsSettings settings = IngameData.Settings.projectileSkinsSettings;

                    string sfx = settings.GetLevelTargetCollisionSfx(weaponSkinType);

                    SoundManager.Instance.PlaySound(sfx);
                }
            }
        }

        #endregion
    }
}
