using Modules.Sound;
using Drawmasters.Effects;
using Drawmasters.Vibration;
using UnityEngine;
using DG.Tweening;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public abstract class ProjectileSmashHandlerComponent : ProjectileComponent
    {
        #region Fields

        private readonly CollisionNotifier projectileCollisionNotifier = default;

        private string fxKeyOnSmash;
        private string sfxKeyOnSmash;

        #endregion



        #region Lifecycle

        public ProjectileSmashHandlerComponent(CollisionNotifier _projectileCollisionNotifier)
        {
            projectileCollisionNotifier = _projectileCollisionNotifier;
        }

        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            projectileCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            projectileCollisionNotifier.OnCustomCollisionEnter2D += ProjectileCollisionNotifier_OnCustomCollisionEnter2D;

            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(type);
            ProjectileSkinsSettings settings = IngameData.Settings.projectileSkinsSettings;

            fxKeyOnSmash = settings.GetEffectOnSmashKey(weaponSkinType);
            sfxKeyOnSmash = settings.GetProjectilesBetweenCollisionSfx(weaponSkinType);
        }


        public override void Deinitialize()
        {
            projectileCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            projectileCollisionNotifier.OnCustomCollisionEnter2D -= ProjectileCollisionNotifier_OnCustomCollisionEnter2D;

            DOTween.Kill(mainProjectile);

            base.Deinitialize();
        }


        protected abstract bool AllowSmash(CollidableObject collidableObject);

        protected virtual void Smash(CollidableObject collidableObject) { }

        private void SmashWithCollidableObject(CollidableObject collidableObject)
        {
            if (collidableObject != null)
            {
                if (AllowSmash(collidableObject))
                {
                    EffectManager.Instance.PlaySystemOnce(fxKeyOnSmash,
                                                          mainProjectile.transform.position,
                                                          mainProjectile.transform.rotation);
                    SoundManager.Instance.PlaySound(sfxKeyOnSmash);

                    VibrationManager.Play(IngameVibrationType.AcidProjectileBroke);

                    mainProjectile.Smash(collidableObject);

                    PhysicsSolutions.ApplySmashSettings(ref mainProjectile, collidableObject.transform.position, collidableObject.Type);

                    Smash(collidableObject);
                }
            }
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject reference, Collider2D collision)
        {
            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();
            SmashWithCollidableObject(collidableObject);
        }


        private void ProjectileCollisionNotifier_OnCustomCollisionEnter2D(GameObject arg1, Collision2D collision)
        {
            CollidableObject collidableObject = collision.gameObject.GetComponent<CollidableObject>();
            SmashWithCollidableObject(collidableObject);
        }

        #endregion
    }
}
