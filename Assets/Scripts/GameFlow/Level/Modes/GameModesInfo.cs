using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Drawmasters.Levels;
using Drawmasters.Statistics;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "GameModesInfo",
                     menuName = NamingUtility.MenuItems.IngameSettings + "GameModesInfo")]
    public class GameModesInfo : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class ModeInfo
        {
            public GameMode mode = default;
            public int neededCompletedChapters = default;
            public int[] excludedFromRandomIndexes = default;
        }


        [Serializable]
        public class WeaponInfo
        {
            public WeaponType type = default;
            public WeaponSettings settings = default;
        }

        #endregion



        #region Fields

        public GameMode firstModeToPlay = default;

        [SerializeField] private ModeInfo[] modesInfos = default;
        [SerializeField] private WeaponInfo[] weaponInfos = default;


        private static readonly Dictionary<GameMode, WeaponType> weaponsMathcing = new Dictionary<GameMode, WeaponType>()
        {
            { GameMode.Draw, WeaponType.Sniper },
            { GameMode.HitmastersLegacySniper, WeaponType.HitmastersSniper },
            { GameMode.HitmastersLegacyShotgun, WeaponType.HitmastersShotgun },
            { GameMode.HitmastersLegacyGravygun, WeaponType.HitmastersGravitygun },
            { GameMode.HitmastersLegacyPortalgun, WeaponType.HitmasteresPortalgun }
        };

        #endregion



        #region Properties

        public GameMode[] Modes => modesInfos.OrderBy(m => m.neededCompletedChapters)
            .Select(m => m.mode).ToArray();

        #endregion



        #region Methods

        public static bool TryConvertModeToWeapon(GameMode mode, out WeaponType weapon)
        {
            bool success = weaponsMathcing.TryGetValue(mode, out weapon);

            return success;
        }
        
        public static bool TryConvertWeaponToMode(WeaponType weapon, out GameMode mode)
        {
            bool success = default;

            mode = GameMode.None;

            foreach(var i in weaponsMathcing)
            {
                success = i.Value == weapon;
                if (success)
                {
                    mode = i.Key;
                    break;
                }
            }

            return success;
        }

        public int GetNeededCompletedChapters(GameMode mode) => GetModeInfo(mode).neededCompletedChapters;


        public int[] GetExcludedIndexes(GameMode mode) => GetModeInfo(mode).excludedFromRandomIndexes;


        public WeaponSettings GetSettings(WeaponType type)
        {
            WeaponSettings result = default;

            WeaponInfo data = weaponInfos.Find(element => element.type == type);

            if (data == null)
            {
                CustomDebug.Log($"No weapon with type {type} in {this}");
            }
            else
            {
                result = data.settings;
            }

            return result;
        }


        private ModeInfo GetModeInfo(GameMode mode)
        {
            ModeInfo findedMode = modesInfos.Find(i => i.mode == mode);

            if (findedMode == null)
            {
                CustomDebug.Log("Missing mode info. Mode = " + mode);
            }

            return findedMode;
        }

        #endregion
    }
}
