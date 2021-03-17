using Drawmasters.Helpers;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ShooterLineAimingDrawer : ShooterAimingDrawer, IDeinitializable
    {
        #region Fields
        
        private SpriteRenderer scopeObject;
        private Color scopeObjectColor;

        private LineRenderer shotRenderer;
        private Gradient shotRendererGradient;

        private readonly float rendererMagnitude;
        private float renderOffset;

        private Vector2[] trajectory;

        #endregion



        #region Properties

        public override Vector2 CurrentDirection { get; protected set; }

        #endregion



        #region Abstract implementation

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

        public ShooterLineAimingDrawer()
        {
            rendererMagnitude = IngameData.Settings.shooter.input.shotRendererMagnitude;
        }

        #endregion



        #region Methods

        public override void Initialize(Transform levelTransform, WeaponType type)
        {
            shotRenderer = Content.Management.CreateShotDirectLineRenderer(levelTransform);

            scopeObject = Content.Management.CreateShooterScope(levelTransform, type);
            scopeObjectColor = scopeObject.color;

            base.Initialize(levelTransform, type);
        }


        public override void Deinitialize()
        {
            base.Deinitialize();

            if (shotRenderer != null)
            {
                Content.Management.DestroyObject(shotRenderer.gameObject);
            }

            if (scopeObject != null)
            {
                Content.Management.DestroyObject(scopeObject.gameObject);
            }
        }


        public override void StartDrawing(Vector2 touchPosition)
        {
            base.StartDrawing(touchPosition);

            if (scopeObject != null)
            {
                CommonUtility.SetObjectActive(scopeObject.gameObject, true);
                scopeObject.transform.position = touchPosition;
            }
        }


        public override void DrawShotDirection(Vector2 startPosition, Vector2 touchPosition)
        {
            base.DrawShotDirection(startPosition, touchPosition);

            if (shotRenderer != null)
            {
                shotRenderer.positionCount = 2;

                CurrentDirection = (touchPosition - startPosition).normalized;
                Vector2 endRendererPoint = startPosition + rendererMagnitude * CurrentDirection;

                shotRenderer.SetPosition(0, startPosition + CurrentDirection * renderOffset);
                shotRenderer.SetPosition(1, endRendererPoint);

                trajectory = new Vector2[] { startPosition, endRendererPoint };
            }

            if (scopeObject != null)
            {
                scopeObject.transform.position = touchPosition;
            }
        }


        public override void ClearDraw(bool isImmediately)
        {
            base.ClearDraw(isImmediately);

            if (shotRenderer != null)
            {
                shotRenderer.positionCount = 0;
            }

            if (scopeObject != null)
            {
                CommonUtility.SetObjectActive(scopeObject.gameObject, false);
            }
        }


        protected override void ApplySettings(WeaponType type)
        {
            WeaponSkinType skinType = GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(type);

            renderOffset = IngameData.Settings.projectileSkinsSettings.GetAimRayOffset(skinType);

            WeaponSettings settings = IngameData.Settings.modesInfo.GetSettings(type);

            if (settings is IProjectileLineRendererSettings lineRendererSettings)
            {
                shotRenderer.startWidth = lineRendererSettings.BeginWidth;
                shotRenderer.endWidth = lineRendererSettings.EndWidth;
                shotRendererGradient = lineRendererSettings.LineGradient;

                shotRenderer.colorGradient = shotRendererGradient;
            }
            else
            {
                CustomDebug.Log($"{settings.GetType().Name} doesn't implement {nameof(IProjectileLineRendererSettings)}.");
            }
        }


        public override void SetReloadVisualEnabled(bool value)
        {
            base.SetReloadVisualEnabled(value);

            if (shotRenderer != null)
            {
                Gradient alphaGradient = value ? GraphicsUtility.GetAlphaGradient() : shotRendererGradient;
                shotRenderer.colorGradient = alphaGradient;
            }

            if (scopeObjectColor != null)
            {
                scopeObject.color = value ? Color.clear : scopeObjectColor;
            }
        }

        #endregion
    }
}
