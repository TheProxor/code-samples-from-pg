using UnityEngine;


namespace Drawmasters.Levels
{
    public sealed class SpeechBubbleLoserHandler : SpeechBubbleRuntimeHandler
    {
        #region Nested Types

        private enum FlipOrientation: byte
        {
            None    = 0,
            Right   = 1,
            Left    = 2
        }

        #endregion



        #region Fields
        [SerializeField] private SpriteRenderer spriteRenderer = default;
        [SerializeField] private Vector2 offset = default;

        private Vector2 direction;

        #endregion



        #region Methods

        public void SetBubbleOrientation()
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            FlipOrientation flipOrientation;

            if (screenPos.x < Screen.width * 0.5f)
            {
                flipOrientation = FlipOrientation.Right;        
                direction = Vector2.one;
            }
            else
            {
                flipOrientation = FlipOrientation.Left;
                direction = new Vector2(-1, 1);
            }

            Flip(flipOrientation);
            UpdatePosition();
        }

        private void UpdatePosition() =>
            transform.localPosition = new Vector3(offset.x * direction.x, offset.y * direction.y, 0);

        private void Flip(FlipOrientation flipOrientation)
        {
            switch(flipOrientation)
            {
                case FlipOrientation.Right:
                    spriteRenderer.flipX = false;
                    break;

                case FlipOrientation.Left:
                     spriteRenderer.flipX = true;
                    break;

                default:
                    spriteRenderer.flipX = false;
                    break;
            }
           
        }

        #endregion
    }
}
