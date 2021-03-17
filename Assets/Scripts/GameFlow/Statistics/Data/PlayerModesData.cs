using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Prefs;


namespace Drawmasters.Statistics.Data
{
    public class PlayerModesData
    {
        #region Fields

        private readonly InfoHolder<OpenModeInfo, GameMode> openModeHolder;

        private readonly List<GameMode> allModes = new List<GameMode>
        {
            GameMode.Draw,
            GameMode.HitmastersLegacyGravygun,
            GameMode.HitmastersLegacyPortalgun,
            GameMode.HitmastersLegacyShotgun,
            GameMode.HitmastersLegacySniper
        };

        #endregion



        #region Properties
        
        public GameMode[] AllModes => allModes.ToArray();

        public GameMode[] Modes => AllModes
                                    .Where(e => !e.IsExcludedFromLoad())
                                    .ToArray();

        public GameMode NextLockedMode => Modes.Where(m => !IsModeOpen(m)).FirstOrDefault();

        public GameMode FirstModeToPlay => Modes.FirstOrDefault();

        public bool IsAllModesOpened => Modes.Where(m => IsModeOpen(m)).Count() == Modes.Length;

        #endregion



        #region Ctor

        public PlayerModesData()
        {
            openModeHolder = new InfoHolder<OpenModeInfo, GameMode>(PrefsKeys.PlayerInfo.OpenModesInfo);
        }

        #endregion



        #region Methods
        
        public bool IsWeaponOpen(WeaponType weapon)
        {
            bool result = default;
            bool isSuccess = GameModesInfo.TryConvertWeaponToMode(weapon, out GameMode mode);

            if (isSuccess)
            {
                result = IsModeOpen(mode);
            }

            return result;
        }


        public bool IsModeOpen(GameMode mode)
        {
            bool isModeOpen = default;

            if (openModeHolder.GetData(mode, out OpenModeInfo modeInfo))
            {
                isModeOpen = modeInfo.isOpen;
            }

            return isModeOpen;
        }


        public void OpenMode(GameMode openMode)
        {
            openModeHolder.SetData(openMode, new OpenModeInfo
            {
                key = openMode,
                isOpen = true
            });
        }
        
        #endregion



        #region Ua

        public void OpenAll()
        {
            foreach(var m in Modes)
            {
                OpenMode(m);
            }
        }

        #endregion
    }
}

