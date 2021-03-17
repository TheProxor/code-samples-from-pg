using Drawmasters.Levels;
using UnityEngine;



namespace Drawmasters.LevelConstructor
{
    public class EditorBackgroundLevelObject : EditorLevelObject
    {
        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default;

        #endregion



        #region Properties

        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponent<SpriteRenderer>();
                }

                return spriteRenderer;
            }
        }

        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            Sprite sprite = IngameData.Settings.commonBackgroundsSettings.FindBackgroundSprite(0);

            if (sprite != null)
            {
                SpriteRenderer.sprite = sprite;
                transform.position = transform.position.SetZ(2.0f);
            }
        }

        #endregion
    }
}
