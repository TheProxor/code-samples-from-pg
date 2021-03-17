using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class WeaponSettings : ScriptableObject, IProjectileSightSettings
    {
        #region Fields

        [SerializeField] private Sprite sightSprite = default;
        [SerializeField] private Color sightColor = default;

        #endregion



        #region Properties

        public abstract ProjectileType ProjectileType { get; }

        #endregion



        #region IProjectileSightSettings

        public Sprite SightSprite => sightSprite;

        public Color SightColor => sightColor;

        #endregion
    }
}
