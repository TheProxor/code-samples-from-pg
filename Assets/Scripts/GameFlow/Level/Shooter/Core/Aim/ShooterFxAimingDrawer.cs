using Drawmasters.ServiceUtil;
using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterFxAimingDrawer : ShooterAimingDrawer
    {
        #region Fields

        private static readonly int raycastMask = ~LayerMask.GetMask(PhysicsLayers.Shooter);

        private GravitygunAimVfx shotRenderer;

        private readonly float rendererMagnitude;
        private float additionalRendererDistanceOnHit;
        private float rendererFxOffset;

        private Vector2[] trajectory;

        #endregion



        #region Properties

        public override Vector2 CurrentDirection { get; protected set; }

        public override Vector2 StartDirection
        {
            get
            {
                Vector2 direction = Vector2.up;

                if (trajectory != null &&
                    trajectory.Length == 2)
                {
                    direction = trajectory[1] - trajectory[0];
                    direction.Normalize();
                }

                return direction;
            }
        }

        public override Vector2[] CurrentProjectileTrajectory => trajectory;

        protected override string UnderFingerFxKey => string.Empty;

        protected override string DrawSfxKey => string.Empty;

        public override float PathDistance => 0f;

        #endregion



        #region Class lifecycle

        public ShooterFxAimingDrawer()
        {
            rendererMagnitude = IngameData.Settings.shooter.input.shotRendererMagnitude;
        }

        #endregion



        #region Methods

        public override void Initialize(Transform levelTransform, WeaponType type)
        {
            shotRenderer = Content.Management.CreateGravyShotLineRenderer(levelTransform);
            
            shotRenderer.Initialize(WeaponSkinType.None);

            base.Initialize(levelTransform, type);

            GravitygunWeapon.OnShouldThrowObject += GravitygunWeapon_OnShouldThrowObject;
            GravitygunWeapon.OnShouldPullObject += GravitygunWeapon_OnShouldPullObject;
        }


        public override void Deinitialize()
        {
            GravitygunWeapon.OnShouldThrowObject -= GravitygunWeapon_OnShouldThrowObject;
            GravitygunWeapon.OnShouldPullObject -= GravitygunWeapon_OnShouldPullObject;

            base.Deinitialize();

            if (shotRenderer != null)
            {
                shotRenderer.Deinitialize();
                Content.Management.DestroyObject(shotRenderer.gameObject);
            }
        }


        public override void StartDrawing(Vector2 touchPosition)
        {
            base.StartDrawing(touchPosition);

            CurrentDirection = touchPosition.normalized;

            shotRenderer.Play();
        }


        public override void DrawShotDirection(Vector2 startPosition, Vector2 touchPosition)
        {
            base.DrawShotDirection(startPosition, touchPosition);

            float rayDistance = rendererMagnitude;

            CurrentDirection = (touchPosition - startPosition).normalized;

            Debug.DrawLine(startPosition, touchPosition, Color.red, 2f);

            RaycastHit2D hit = Physics2D.Raycast(startPosition, CurrentDirection, float.MaxValue, raycastMask);

            if (hit.collider != null)
            {
                rayDistance = hit.distance + additionalRendererDistanceOnHit;
            }

            trajectory = new Vector2[] { startPosition, startPosition + CurrentDirection * rayDistance };

            Debug.DrawLine(startPosition, CurrentDirection * rayDistance, Color.green, 2f);

            if (shotRenderer != null)
            {
                shotRenderer.RecalculateDistance(rayDistance - rendererFxOffset);
                shotRenderer.transform.position = startPosition + CurrentDirection * rendererFxOffset;

                float angle = Vector2.SignedAngle(Vector2.right, CurrentDirection);

                shotRenderer.transform.eulerAngles = shotRenderer.transform.eulerAngles.SetZ(angle);
            }
        }

               
        public override void ClearDraw(bool isImmediately)
        {
            base.ClearDraw(isImmediately);

            if (shotRenderer != null)
            {
                shotRenderer.StopDrawing();
            }
        }


        protected override void ApplySettings(WeaponType type)
        {
            WeaponSettings settings = IngameData.Settings.modesInfo.GetSettings(type);

            //TODO hardcoded values
            rendererFxOffset = 0f;
            additionalRendererDistanceOnHit = 0f;
        }


        private void GravitygunWeapon_OnShouldPullObject(Transform root, Vector2 dir) =>
            ClearDraw(true);
        

        private void GravitygunWeapon_OnShouldThrowObject(Vector2 dir) =>
            ClearDraw(true);
        
        #endregion
    }
}
