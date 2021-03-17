using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using System;
using System.Linq;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class ProjectilePortalComponent : ProjectileComponent
    {
        #region Fields

        public static event Action OnProjectileDestroyed;
        public static event Action<Vector3, Vector3, Vector3> OnMonolithCollision; // projectile position !on monolith line!, left border pos, right border pos

        #endregion



        #region Overrided methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainProjectile.OnShouldDestroy += MainProjectile_OnShouldDestroy;
        }


        public override void Deinitialize()
        {
            mainProjectile.OnShouldDestroy -= MainProjectile_OnShouldDestroy;

            OnProjectileDestroyed?.Invoke();

            base.Deinitialize();
        }

        #endregion



        #region Methods

        private void CheckProjectilePlace(HitmastersPortalgunProjectile checkProjectile)
        {
            Collider2D[] castColliders = Physics2D.OverlapCircleAll(checkProjectile.transform.position,
                                                                    checkProjectile.CastRadius);

            LevelObjectMonolith monolith = castColliders
                .Where(e => e.GetComponent<CollidableObject>() != null &&
                            e.GetComponent<CollidableObject>().Monolith != null)
                .Select(e => e.GetComponent<CollidableObject>().Monolith)
                .FirstOrDefault();


            Collider2D[] spikesCastColliders = Physics2D.OverlapCircleAll(checkProjectile.transform.position,
                                                                    checkProjectile.MonolithSpikesCastRadius);

            Spikes spikes = spikesCastColliders
                .Where(e => e.GetComponent<CollidableObject>() != null &&
                            (e.GetComponent<CollidableObject>().PhysicalLevelObject as Spikes) != null)
                .Select(e => e.GetComponent<CollidableObject>().PhysicalLevelObject as Spikes)
                .FirstOrDefault();

            PortalObject anotherPortal = spikesCastColliders
              .Where(e => e.GetComponent<CollidableObject>() != null &&
                          (e.GetComponent<CollidableObject>().PortalObject) != null)
              .Select(e => e.GetComponent<CollidableObject>().PortalObject)
              .FirstOrDefault();

            if (anotherPortal != null ||
                (spikes != null && spikes.PhysicalData.type == PhysicalLevelObjectType.Monolit))
            {
                PlayMissEffect(checkProjectile.transform.position);
                return;
            }

            if (monolith != null)
            {
                float minDistance = float.MaxValue;

                Vector3 leftNearPointPosition = Vector3.zero;
                Vector3 rightNearPointPosition = Vector3.zero;

                for (int i = 0; i < monolith.Spline.GetPointCount(); i++)
                {
                    Vector3 splineWorldPosition = monolith.transform.TransformPoint(monolith.Spline.GetPosition(i));

                    int nextPointNext = i + 1 >= monolith.Spline.GetPointCount() ? 0 : i + 1;
                    Vector3 nextSplineWorldPosition = monolith.transform.TransformPoint(monolith.Spline.GetPosition(nextPointNext));

                    float distance = CommonUtility.CalculateDistanceFromPointToSegment(checkProjectile.transform.position, splineWorldPosition, nextSplineWorldPosition);

                    if (distance < minDistance)
                    {
                        leftNearPointPosition = splineWorldPosition;
                        rightNearPointPosition = nextSplineWorldPosition;


                        minDistance = distance;
                    }
                }

                Vector3 portalPosition = CommonUtility.NearestPointOnSegment(leftNearPointPosition, rightNearPointPosition, checkProjectile.transform.position);

#if UNITY_EDITOR
                CommonUtility.DrawCircle(leftNearPointPosition, 1f, 10, Color.green, true, 1.0f);
                CommonUtility.DrawCircle(rightNearPointPosition, 1f, 10, Color.green, true, 1.0f);
                CommonUtility.DrawCircle(portalPosition, 1f, 10, Color.red, true, 1.0f);
#endif

                float monolithWidth = Vector3.Distance(leftNearPointPosition, rightNearPointPosition);
                float minPortalWidth = IngameData.Settings.portalsSettings.minPortalWidthForCollision;

                if (monolithWidth < minPortalWidth)
                {
                    PlayMissEffect(portalPosition);

                    return;
                }

                PlayHitEffect(portalPosition);
                OnMonolithCollision?.Invoke(portalPosition, leftNearPointPosition, rightNearPointPosition);
            }
        }

        private void PlayMissEffect(Vector3 position)
        {
            WeaponSkinType weaponSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(WeaponType.HitmasteresPortalgun);
            
            string missEffectKey = PortalController.createType == PortalObject.Type.First ? EffectKeys.FxWeaponPortalGunBulletGreenMiss :
                                                                                            EffectKeys.FxWeaponPortalGunBulletOrangeMiss;
            EffectManager.Instance.PlaySystemOnce(missEffectKey, position);
        }


        private void PlayHitEffect(Vector3 position)
        {
            return;

            //TODO this logic wasn't implemented in Hitmasters         
        }

        #endregion



        #region Events handlers

        private void MainProjectile_OnShouldDestroy(Projectile otherProjectile)
        {
            if (mainProjectile.Equals(otherProjectile))
            {
                // TODO: maybe check collision rather than physics cast?
                CheckProjectilePlace(mainProjectile as HitmastersPortalgunProjectile);
            }
        }

        #endregion
    }
}
