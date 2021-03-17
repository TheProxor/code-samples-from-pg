using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.Prefs;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;


namespace Drawmasters.Statistics.Data
{
    public partial class PlayerData : BaseDataSaveHolder<PlayerData.Data>
    {
        #region Helpers

        [Serializable]
        public class Data : BaseDataSaveHolderData
        {
            public ShooterSkinType currentSkin = default;
            public GameMode lastPlayedMode = default;
            public PetSkinType currentPetSkin = default;
            public Dictionary<WeaponType, WeaponSkinType> currentWeaponSkins = default;
            public Dictionary<GameMode, int> currentModeInfos = default;
            public Dictionary<PetSkinType, float> currentPetChargePoints = default;
            public bool isBloodEnabled = default;


            public Data()
            {
                RestoreOldDataValues();
            }


            private void RestoreOldDataValues()
            {
                string playerSkinKey = PrefsKeys.PlayerInfo.CurrentPlayerSkin;

                if (CustomPlayerPrefs.HasKey(playerSkinKey))
                {
                    currentSkin = CustomPlayerPrefs.GetEnumValue(playerSkinKey, DefaultShooterSkin);
                }

                string petSkinKey = PrefsKeys.PlayerInfo.CurrentPetSkin;
                if (CustomPlayerPrefs.HasKey(petSkinKey))
                {
                    currentPetSkin = CustomPlayerPrefs.GetEnumValue(petSkinKey, DefaultPetSkin);
                }

                string lastPlayedModeKey = PrefsKeys.PlayerInfo.LastPlayedMode;
                if (CustomPlayerPrefs.HasKey(lastPlayedModeKey))
                {
                    lastPlayedMode = (GameMode)CustomPlayerPrefs.GetInt(lastPlayedModeKey);
                }

                string isBloodEnabledKey = PrefsKeys.PlayerInfo.IsBloodEnabled;
                if (CustomPlayerPrefs.HasKey(isBloodEnabledKey))
                {
                    isBloodEnabled = CustomPlayerPrefs.GetBool(isBloodEnabledKey, false & isUaBloodEnabled);
                }

                currentWeaponSkins = new Dictionary<WeaponType, WeaponSkinType>(DefaultWeaponSkins);
                currentModeInfos = new Dictionary<GameMode, int>(DefaultModeInfos);
                currentPetChargePoints = new Dictionary<PetSkinType, float>(DefaultPetSkinCharge);

                RestoreOldWeaponSkinsInfos();
                RestoreOldModeInfos();


                void RestoreOldModeInfos()
                {

                    ModeHolder modeHolder = new ModeHolder(PrefsKeys.PlayerInfo.LevelModesProgress);

                    foreach (var modeData in DefaultModeInfos)
                    {
                        if (modeHolder.GetData(modeData.Key, out ModeInfo modeInfo))
                        {
                            currentModeInfos[modeData.Key] = modeInfo.index;
                        }
                    }
                }


                void RestoreOldWeaponSkinsInfos()
                {
                    WeaponSkinHolder weaponSkinHolder = new WeaponSkinHolder(PrefsKeys.PlayerInfo.WeaponSkinInfoUpdate);
                    foreach (var weaponSkinData in DefaultWeaponSkins)
                    {
                        if (weaponSkinHolder.GetData(weaponSkinData.Key, out WeaponSkinInfo info))
                        {
                            currentWeaponSkins[weaponSkinData.Key] = info.skinType;
                        }
                    }
                }
            }
        }

        #endregion



        #region Events

        public event Action OnWeaponSkinSetted;
        public event Action OnShooterSkinSetted;
        public event Action OnPetSkinSetted;

        #endregion



        #region Fields

        private readonly ILevelEnvironment levelEnvironment;
        private readonly IBackgroundService backgroundService;

        public static readonly ShooterSkinType DefaultShooterSkin = ShooterSkinType.Archer;
        public static readonly PetSkinType DefaultPetSkin = PetSkinType.None;

        private static readonly Dictionary<WeaponType, WeaponSkinType> DefaultWeaponSkins = new Dictionary<WeaponType, WeaponSkinType>
            {
                { WeaponType.Sniper,                WeaponSkinType.BowDefault          },
                { WeaponType.HitmastersSniper,      WeaponSkinType.SnipergunDefault    },
                { WeaponType.HitmastersShotgun,     WeaponSkinType.ShotgunDefault      },
                { WeaponType.HitmastersGravitygun,  WeaponSkinType.GravigunDefault     },
                { WeaponType.HitmasteresPortalgun,  WeaponSkinType.PortalgunDefault    }
            };

        private static readonly Dictionary<GameMode, int> DefaultModeInfos = new Dictionary<GameMode, int>
            {
                {   GameMode.Draw,                        default   },
                {   GameMode.HitmastersLegacyAcid,        default   },
                {   GameMode.HitmastersLegacyGravygun,    default   },
                {   GameMode.HitmastersLegacyPortalgun,   default   },
                {   GameMode.HitmastersLegacyShotgun,     default   },
                {   GameMode.HitmastersLegacySniper,      default   },
            };

        private static readonly Dictionary<PetSkinType, float> DefaultPetSkinCharge = new Dictionary<PetSkinType, float>
            {
                {   PetSkinType.ParrotPirate,   default   },
                {   PetSkinType.PartyCat,       default   },
                {   PetSkinType.PixelChicken,   default   },
                {   PetSkinType.RubberDuck,     default   },
                {   PetSkinType.SmallDragon,    default   },
                {   PetSkinType.SphereDron,     default   },
                {   PetSkinType.VampireBat,     default   },
            };

        #endregion



        #region Properties

        public ShooterSkinType CurrentSkin
        {
            get => data.currentSkin;
            set
            {
                data.currentSkin = value;
                SaveData();
                OnShooterSkinSetted?.Invoke();
            }
        }


        public PetSkinType CurrentPetSkin
        {
            get => data.currentPetSkin;
            set
            {
                data.currentPetSkin = value;
                SaveData();
                OnPetSkinSetted?.Invoke();
            }
        }


        public GameMode LastPlayedMode
        {
            get
            {
                GameMode result = GameMode.None;

                if (data.lastPlayedMode != GameMode.None)
                    result = data.lastPlayedMode;
                else
                    result = DefaultModeInfos.Keys.FirstOrDefault();

                return result;
            }
            private set
            {
                data.lastPlayedMode = value;
                SaveData();
            }
        }

        protected override string SaveKey => PrefsKeys.PlayerInfo.PlayerDataUpdated;

        public static bool WasBloodChanged => CustomPlayerPrefs.HasKey(PrefsKeys.PlayerInfo.IsBloodEnabled);

        public bool IsBloodEnabled
        {
            get => data.isBloodEnabled;
            set
            {
                data.isBloodEnabled = value;
                SaveData();
            }
        }

        #endregion



        #region Ctor

        public PlayerData(ILevelEnvironment _levelEnvironment,
                          IBackgroundService _backgroundService) 
        {
            levelEnvironment = _levelEnvironment;
            backgroundService = _backgroundService;

            if (data.currentSkin == ShooterSkinType.None)
            {
                data.currentSkin = DefaultShooterSkin;
                SaveData();
            }

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region Methods

        public void ResetSkins()
        {
            // hotfix set without callbacks
            data.currentSkin = DefaultShooterSkin;
            data.currentPetSkin = DefaultPetSkin;
            data.currentWeaponSkins = DefaultWeaponSkins;

            SaveData();
        }


        public void RecordLastPlayedMode(GameMode mode) => LastPlayedMode = mode;

        public void SetWeaponSkin(WeaponType weaponType, WeaponSkinType skinType)
        {
            if (!data.currentWeaponSkins.ContainsKey(weaponType))
            {
                data.currentWeaponSkins.Add(weaponType,skinType);
                CustomDebug.LogError($"Player Data weapons skins collection don't contains weapon type '{weaponType}'");
                return;
            }

            data.currentWeaponSkins[weaponType] = skinType;
            SaveData();
            OnWeaponSkinSetted?.Invoke();
        }


        public WeaponSkinType GetCurrentWeaponSkin(WeaponType weaponType)
        {
            WeaponSkinType result;

            if (!data.currentWeaponSkins.ContainsKey(weaponType))
            {
                data.currentWeaponSkins.Add(weaponType, default);
                CustomDebug.LogError($"Player Data weapons skins collection don't contains weapon type '{weaponType}'");
            }

            result = data.currentWeaponSkins[weaponType];

            return result;
        }

        public int GetModeCurrentIndex(GameMode gameMode)
        {
            int result;

            if (!data.currentModeInfos.ContainsKey(gameMode))
            {
                data.currentModeInfos.Add(gameMode, default);
                CustomDebug.LogError($"Player Data game infos collection don't contains game mode type '{gameMode}'");
            }

            result = data.currentModeInfos[gameMode];

            return result;
        }

        public void SetModeInfo(GameMode gameMode, int currentLevelIndex)
        {
            if (!data.currentModeInfos.ContainsKey(gameMode))
            {
                data.currentModeInfos.Add(gameMode, currentLevelIndex);
                CustomDebug.LogError($"Player Data game infos collection don't contains game mode type '{gameMode}'");
                return;
            }

            data.currentModeInfos[gameMode] = currentLevelIndex;
            SaveData();
        }


        public float GetPetCharge(PetSkinType petSkinType)
        {
            if (!data.currentPetChargePoints.ContainsKey(petSkinType))
            {
                CustomDebug.Log($"Attempt to set pet charge for not valid <b>{petSkinType}</b> pet skin");
                return 0.0f;
            }

            return data.currentPetChargePoints[petSkinType];
        }


        public void SetPetCharge(PetSkinType petSkinType, float value)
        {
            if (!data.currentPetChargePoints.ContainsKey(petSkinType))
            {
                CustomDebug.Log($"Attempt to set pet charge for not valid <b>{petSkinType}</b> pet skin");
                return;
            }

            data.currentPetChargePoints[petSkinType] = value;
            SaveData();
        }


        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.Finished)
            {
                LevelProgress progress = levelEnvironment.Progress;
                LevelContext levelContext = levelEnvironment.Context;

                LevelResult levelResult = progress.LevelResultState;

                bool shouldUpLevel = levelResult.IsLevelAccomplished() ||
                                    (levelContext.Mode.IsHitmastersLiveOps() && levelResult.IsLevelOrProposalAccomplished());

                if (shouldUpLevel)
                {
                    int index = GetModeCurrentIndex(levelContext.Mode);

                    SetModeInfo(levelContext.Mode, index + 1);
                }
            }
        }

        #endregion
    }
}
