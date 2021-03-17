using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Drawmasters.Levels;

namespace Drawmasters
{
    public static class GameModeExtension
    {
        #region Fields

        private static readonly HashSet<GameMode> SceneModes
            = new HashSet<GameMode>
            { GameMode.ShopScene,
              GameMode.RouletteScene,
              GameMode.MenuScene,
              GameMode.ForcemeterScene,
              GameMode.PremiumShopScene
            };

        private static readonly HashSet<GameMode> ProposalSceneModes
            = new HashSet<GameMode>
            { GameMode.ShopScene,
              GameMode.RouletteScene,
              GameMode.MenuScene,
              GameMode.ForcemeterScene,
              GameMode.PremiumShopScene };

        public static readonly HashSet<GameMode> ExcludedFromLoad =
            new HashSet<GameMode>
            { GameMode.UaLandscapeMode,
              GameMode.ForcemeterScene,
              GameMode.MenuScene,
              GameMode.RouletteScene
            };


        public static readonly HashSet<GameMode> HitmastersLiveOps =
            new HashSet<GameMode>
            { GameMode.HitmastersLegacyAcid,
              GameMode.HitmastersLegacyGravygun,
              GameMode.HitmastersLegacyPortalgun,
              GameMode.HitmastersLegacyShotgun,
              GameMode.HitmastersLegacySniper
            };


        #endregion



        #region Public methods

        public static bool IsModeOpen(this GameMode mode)
            => GameServices.Instance.PlayerStatisticService.ModesData.IsModeOpen(mode);

        public static int GetCurrentLevelIndex(this GameMode mode)
            => GameServices.Instance.PlayerStatisticService.PlayerData.GetModeCurrentIndex(mode);

        public static int GetFinishedLevels(this GameMode mode)
            => GameServices.Instance.CommonStatisticService.GetLevelsFinishedCount(mode);

        public static void ResetFinishedLevels(this GameMode mode)
            => GameServices.Instance.CommonStatisticService.ResetLevelsFinishedCount(mode);

        public static bool IsSceneMode(this GameMode mode) =>
            SceneModes.Contains(mode);

        public static bool IsProposalSceneMode(this GameMode mode) =>
            ProposalSceneModes.Contains(mode);

        public static bool IsExcludedFromLoad(this GameMode mode) =>
            ExcludedFromLoad.Contains(mode);


        public static bool IsHitmastersLiveOps(this GameMode mode) =>
            HitmastersLiveOps.Contains(mode);

        public static bool IsPlayable(this GameMode mode) 
            => mode == GameMode.Draw || 
               mode.IsHitmastersLiveOps();

        public static bool IsDrawingMode(this GameMode mode) 
            => mode == GameMode.Draw || 
               mode == GameMode.UaLandscapeMode;

        public static List<string> GetNames<T>() => new List<string>(Enum.GetNames(typeof(T)));

        public static int ToNameIndex<T>(this T value) where T : struct, IConvertible
            => GetNames<T>().IndexOf(Enum.GetName(typeof(T), value));

        public static T FromNameIndex<T>(in int value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            if (-1 < value && value < GetNames<T>().Count)
            {
                return (T)Enum.Parse(typeof(T), GetNames<T>()[value]);
            }

            return default;
        }

        public static string UiHeaderText(this GameMode thisMode)
        {
            string result;
            
            bool isModeOpen = GameServices.Instance.PlayerStatisticService.ModesData.IsModeOpen(thisMode);
            if (isModeOpen)
            {
                int index = GameServices.Instance.CommonStatisticService.GetLevelsFinishedCount(thisMode);

                result = $"{index + 1}";
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        #endregion
    }
}

