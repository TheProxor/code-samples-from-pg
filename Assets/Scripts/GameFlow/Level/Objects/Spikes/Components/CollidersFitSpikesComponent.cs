using UnityEngine;


namespace Drawmasters.Levels
{
    public class CollidersFitSpikesComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default;
        [SerializeField] private BoxCollider2D[] boxColliders = default;

        #endregion



        #region Methods

        public override void Enable()
        {
            foreach (var boxCollider in boxColliders)
            {
                boxCollider.size = new Vector2(spriteRenderer.size.x, boxCollider.size.y);
            }
        }


        public override void Disable()
        {
        }

        #endregion
    }
}
