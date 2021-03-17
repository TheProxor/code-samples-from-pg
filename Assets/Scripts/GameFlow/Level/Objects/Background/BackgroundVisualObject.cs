using Drawmasters.Levels.Data;
using Drawmasters.Levels.Order;
using Drawmasters.ServiceUtil;
using Drawmasters.Ua;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class BackgroundVisualObject : LevelObject
    {
        #region Nested types

        private class UaData
        {
            public SpriteDrawMode mode = default;
            public Sprite sprite = default;
            public Color color = default;
            public Vector2 size = default;
        }

        #endregion



        #region Fields

        [SerializeField] private SpriteRenderer spriteRenderer = default;
        
        private UaData uaData;
        
        private ColorProfile loadedColorProfile;

        #endregion



        #region Methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            transform.position = Vector3.zero;
        }


        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            base.StartGame(mode, weaponType, levelTransform);

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            loadedColorProfile = IngameData.Settings.colorProfilesSettings.GetProfile(context.ColorProfileIndex);

            InitializeSprite(loadedColorProfile.backgroundIndex);

            if (uaData != null)
            {
                SetRendererUaData();
            }

            PuzzlemastersDevContent.OnShouldResetLevelVisual += PuzzlemastersDevContent_OnShouldResetLevelVisual;
            PuzzlemastersDevContent.OnSettedPortraitOrientationEnabled += RefreshScale;

            RefreshScale(IngameCamera.IsPortrait);

        }


        public override void FinishGame()
        {
            PuzzlemastersDevContent.OnShouldResetLevelVisual -= PuzzlemastersDevContent_OnShouldResetLevelVisual;
            PuzzlemastersDevContent.OnSettedPortraitOrientationEnabled -= RefreshScale;

            base.FinishGame();
        }


        public void ChangeBackgroundColor(Color color, Vector2 spriteSize)
        {
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = spriteSize;
            spriteRenderer.sprite = IngameData.Settings.commonBackgroundsSettings.WhiteSprite;
            spriteRenderer.color = color;
            spriteRenderer.transform.localScale = Vector3.one;

        }


        private void SetRendererUaData()
        {
            spriteRenderer.drawMode = uaData.mode;
            spriteRenderer.color = uaData.color;
            spriteRenderer.sprite = uaData.sprite;
            spriteRenderer.size = uaData.size;

            spriteRenderer.transform.localScale = Vector3.one;
        }


        public void RefreshUaData()
        {
            uaData = uaData ?? new UaData();

            uaData.mode = spriteRenderer.drawMode;
            uaData.size = spriteRenderer.size;
            uaData.sprite = spriteRenderer.sprite;
            uaData.color = spriteRenderer.color;
        }


        public void InitializeSprite(int backgroundIndex)
        {
            CommonBackgroundsSettings settings = IngameData.Settings.commonBackgroundsSettings;

            Sprite spriteToSet = settings.FindBackgroundSprite(backgroundIndex);

            if (spriteToSet != null)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Simple;
                spriteRenderer.sprite = spriteToSet;                
                spriteRenderer.transform.localScale = Vector3.one;
            }
        }

        #endregion



        #region Events handlers

        private void PuzzlemastersDevContent_OnShouldResetLevelVisual()
        {
            InitializeSprite(loadedColorProfile.backgroundIndex);

            uaData = null;
        }


        private void RefreshScale(bool isPortraitOrientation)
        {
            float sizeMultiplier = isPortraitOrientation ? 1.0f : 2.0f;
            spriteRenderer.transform.localScale = Vector3.one * sizeMultiplier;
        }

        #endregion
    }
}
