using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class ProjectileSkinComponent : ProjectileComponent
    {
        #region Fields

        private readonly SpriteRenderer mainRenderer;

        #endregion



        #region Properties

        protected abstract Sprite MainRendererSprite { get; }

        #endregion



        #region Class lifecycle

        public ProjectileSkinComponent(SpriteRenderer _mainRenderer)
        {
            mainRenderer = _mainRenderer;
        }


        #endregion



        #region Methods

        public override void Initialize(Projectile mainProjectileValue, WeaponType type)
        {
            base.Initialize(mainProjectileValue, type);

            mainRenderer.sprite = MainRendererSprite;
        }

        #endregion
    }
}
