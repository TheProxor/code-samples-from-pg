using Drawmasters.Levels.Data;
using Drawmasters.Levels.Order;
using Drawmasters.Ua;
using UnityEngine;


namespace Drawmasters.Levels
{
    public partial class LevelObjectMonolith
    {
        #region Properties

        public static WeaponType UaWeaponTypeMonolith { get; set; } = WeaponType.None;

        public static bool UseColoredFillTexture { get; set; }

        public static bool UseColoredEdgeTexture { get; set; }

        #endregion



        #region Methods

        private void SubscribeOnUaEvents()
        {
            //PuzzlemastersDevContent.OnShouldChangeMonolith += RenderMonolith;
            PuzzlemastersDevContent.OnShouldResetLevelVisual += PuzzlemastersDevContent_OnShouldResetLevelVisual;
        }


        private void UnsubscribeFromUaEvents()
        {
            //PuzzlemastersDevContent.OnShouldChangeMonolith -= RenderMonolith;
            PuzzlemastersDevContent.OnShouldResetLevelVisual -= PuzzlemastersDevContent_OnShouldResetLevelVisual;
        }


        private void SetEdgeSprite(Color color)
        {
            
        }

        #endregion



        #region Events handlers

        private void PuzzlemastersDevContent_OnShouldResetLevelVisual()
        {
            UaWeaponTypeMonolith = WeaponType.None;
            currentWeaponType = WeaponType;

            UseColoredFillTexture = false;
            UseColoredEdgeTexture = false;

            RenderMonolith();

            LevelContext context = levelEnvironment.Context;
            ColorProfile colorProfile = IngameData.Settings.colorProfilesSettings.GetProfile(context.ColorProfileIndex);

            RepaintFillTexture(colorProfile.monolithFillColor, colorProfile.monolithFillType);
            RepaintEdgeTexture(colorProfile.monolithEdgeColor);
        }

        #endregion
    }
}
