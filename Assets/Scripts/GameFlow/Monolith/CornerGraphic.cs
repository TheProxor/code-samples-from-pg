using UnityEngine;

namespace Drawmasters.Monolith
{
    public class CornerGraphic : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default;

        #endregion
        


        #region Methods

        public void Initialize(Sprite newSprite, Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newSprite;
                spriteRenderer.color = color;
            }
        }

        #endregion



        #region Editor

        private void Reset()
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        #endregion
    }
}
