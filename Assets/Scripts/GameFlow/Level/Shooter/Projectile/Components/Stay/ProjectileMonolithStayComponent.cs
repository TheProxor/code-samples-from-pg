using Drawmasters.Effects;
using Drawmasters.Monolith;
using Drawmasters.ServiceUtil;
using Drawmasters.Vibration;
using Modules.Sound;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileMonolithStayComponent : ProjectileStayComponent
    {
        #region Fields

        private const float minAngleToStayInMonolith = 60.0f;
        private const float minVelocityMagnitudeToStay = float.MinValue;

        private bool wasProjectileSmashed;

        private string fxKeyOnSmash;
        private string sfxKeyOnSmash;

        #endregion



        #region Properties

        protected override string FxKeyOnStop => string.Empty;

        protected override string SfxKeyOnStop => string.Empty;

        #endregion



        #region Class lifecycle

        public ProjectileMonolithStayComponent(CollisionNotifier _projectileCollisionNotifier) : base(_projectileCollisionNotifier)
        {
        }


        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            wasProjectileSmashed = false;

            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(type);
            ProjectileSkinsSettings settings = IngameData.Settings.projectileSkinsSettings;

            fxKeyOnSmash = settings.GetEffectOnSmashKey(weaponSkinType);
            sfxKeyOnSmash = settings.GetProjectilesBetweenCollisionSfx(weaponSkinType);

            ProjectileSmashApplyComponent.OnSmashProjectile += ProjectileSmashComponent_OnSmashProjectile;
        }


        public override void Deinitialize()
        {
            ProjectileSmashApplyComponent.OnSmashProjectile -= ProjectileSmashComponent_OnSmashProjectile;

            base.Deinitialize();
        }


        protected override bool CanStayProjectileOnCollision(CollidableObject collidableObject, out Vector3 stayPosition)
        {
            bool result = false;
            stayPosition = default;

            if (collidableObject.Monolith != null)
            {
                Vector3 arrowVisualDirection = (Quaternion.Euler(0, 0, mainProjectile.transform.eulerAngles.z) * Vector2.right).normalized;
                (Vector3, Vector3) leftAndRightPosition = MonolithUtility.GetLeftAndRightNearPoint(collidableObject.Monolith, mainProjectile.MainRigidbody2D.position);
                Vector3 positionOnMonolith = CommonUtility.NearestPointOnSegment(leftAndRightPosition.Item1, leftAndRightPosition.Item2, mainProjectile.MainRigidbody2D.position);
                stayPosition = mainProjectile.transform.position + (arrowVisualDirection * offset);

#if UNITY_EDITOR
                CommonUtility.DrawCircle(positionOnMonolith, 1f, 10, Color.red, true, 10.0f);
#endif

                if (CommonUtility.IsDirectedOnAnotherVector(leftAndRightPosition.Item2 - leftAndRightPosition.Item1, arrowVisualDirection))
                {
                    float leftAngle = Vector2.Angle(leftAndRightPosition.Item2 - leftAndRightPosition.Item1, arrowVisualDirection);
                    float rightAngle = Vector2.Angle(leftAndRightPosition.Item1 - leftAndRightPosition.Item2, arrowVisualDirection);

                    float minAngle = Mathf.Min(leftAngle, rightAngle);

                    result = mainProjectile.MainRigidbody2D.velocity.magnitude > minVelocityMagnitudeToStay &&
                             minAngle > minAngleToStayInMonolith &&
                             minAngle < 2.0f * minAngleToStayInMonolith;
                }

                if (!wasProjectileSmashed && !result)
                {
                    EffectManager.Instance.PlaySystemOnce(fxKeyOnSmash,
                                                          mainProjectile.transform.position,
                                                          mainProjectile.transform.rotation);
                    SoundManager.Instance.PlaySound(sfxKeyOnSmash);

                    VibrationManager.Play(IngameVibrationType.AcidProjectileBroke);

                    mainProjectile.Smash(collidableObject);
                }
            }

            return result;
        }

        #endregion



        #region Events handlers

        private void ProjectileSmashComponent_OnSmashProjectile(Projectile anotherProjectile)
        {
            if (mainProjectile == anotherProjectile)
            {
                wasProjectileSmashed = true;
            }
        }

        #endregion
    }
}
