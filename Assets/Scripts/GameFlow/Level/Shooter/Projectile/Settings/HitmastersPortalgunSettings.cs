using Sirenix.OdinInspector;
using UnityEngine;

namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "HitmastersPortalgunSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "ModeInfo/HitmastersPortalgunSettings")]
    public class HitmastersPortalgunSettings : WeaponSettings,
        IProjectileLineRendererSettings,
        IProjectileSpeedSettings
    {
        #region Fields

        [SerializeField] private float projectileSpeed = default;

        [Header("Trajectory")]
        [MinValue(0.0f)] public float shotRendererWidth = default;
        public Gradient shotRendererGradient = default;

        #endregion



        #region Overrided

        public override ProjectileType ProjectileType => ProjectileType.HitmastersPortalgun;

        #endregion



        #region IProjectileLineRendererSettings

        public float BeginWidth => shotRendererWidth;

        public float EndWidth => shotRendererWidth;

        public Gradient LineGradient => shotRendererGradient;

        #endregion



        #region IProjectileSpeedSettings

        public float Speed => projectileSpeed;

        #endregion
    }
}
