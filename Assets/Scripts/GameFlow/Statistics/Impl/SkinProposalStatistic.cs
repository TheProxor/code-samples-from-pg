using System.Linq;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.Statistics
{
    public class SkinProposalStatistic : IShowsCount
    {
        #region Fields

        private readonly PlayerShooterSkinFactorLoader skinFactorLoader;

        private readonly ILevelEnvironment levelEnvironment;
        private readonly IShopService shopService;
        private readonly IAbTestService abTestService;

        private readonly float skinDelta;

        #endregion



        #region Properties

        public bool CanPropose
        {
            get
            {
                bool result = true;
                
                result &= abTestService.CommonData.isVideoSkinProposalAvailable;
                result &= CurrentSkinTypeToPropose != ShooterSkinType.None;
                result &= !levelEnvironment.Context.Mode.IsHitmastersLiveOps();
                
                return result;
            }
        }

        public ShooterSkinType CurrentSkinTypeToPropose { get; private set; }

        public float CurrentSkinProgress
        {
            get => CustomPlayerPrefs.GetFloat(PrefsKeys.PlayerInfo.CurrentSkinProgess);
            private set => CustomPlayerPrefs.SetFloat(PrefsKeys.PlayerInfo.CurrentSkinProgess, value);
        }


        private ShooterSkinType LastProposedSkinType
        {
            get => CustomPlayerPrefs.GetEnumValue(PrefsKeys.PlayerInfo.LastResultSkinProgressType, ShooterSkinType.None);
            set => CustomPlayerPrefs.SetEnumValue(PrefsKeys.PlayerInfo.LastResultSkinProgressType, value);
        }


        public float SkinProgressDelta
        {
            get
            {
                float result = skinDelta;
                
                LevelContext context = levelEnvironment.Context;
                if (context != null)
                {
                    bool isSpecialLevel = context.LevelLocalIndex == Level.DoubleSkinProgressLevelIndex;
                    if (isSpecialLevel)
                    {
                        result *= 2f;
                    }
                }

                return result;
            }
        }


        public bool CanClaimSkin => (CurrentSkinProgress + SkinProgressDelta) >= 1f && CurrentSkinTypeToPropose != ShooterSkinType.None;


        private void GenerateNextProposal()
        {
            ShowsCount++;
            GenerateCurrentSkinTypeToPropose();
        }


        private void GenerateCurrentSkinTypeToPropose()
        {
            ShooterSkinType[] skins = IngameData.Settings.shooterSkinsSettings.FindSkinsTypeForProgress(ShowsCount - 1, shopService);
            
            bool isOnlyLastproposedContains = skins.Length == 1 && skins.First() == LastProposedSkinType;
            ShooterSkinType current = isOnlyLastproposedContains ?
                LastProposedSkinType : skins.Where(e => e != LastProposedSkinType).ToArray().RandomObject();

            CurrentSkinTypeToPropose = current;
        }

        #endregion



        #region Ctor

        public SkinProposalStatistic(ILevelEnvironment _levelEnvironment,
                                     IAbTestService _abTestService,
                                     IShopService _shopService)
        {
            abTestService = _abTestService;
            
            skinFactorLoader = new PlayerShooterSkinFactorLoader(abTestService);
            skinFactorLoader.Load();
            skinDelta = skinFactorLoader.LoadedFactor;

            levelEnvironment = _levelEnvironment;
            shopService = _shopService;

            shopService.ShooterSkins.OnOpened += ShooterSkins_OnOpened;

            if (ShowsCount == 0)
            {
                GenerateNextProposal();
            }
            else
            {
                GenerateCurrentSkinTypeToPropose();
            }

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region Public methods

        public void ClaimCurrentSkin()
        {
            ShooterSkinType skin = CurrentSkinTypeToPropose;

            shopService.ShooterSkins.Open(skin);

            //TODO refactor
            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin = skin;

            CurrentSkinProgress = 0f;

            GenerateNextProposal();
        }

        public void SkipSkin()
        {
            CurrentSkinProgress = 0f;
            LastProposedSkinType = CurrentSkinTypeToPropose;

            GenerateNextProposal();
        }

        #endregion



        #region Private methods

        private void LevelComplete(bool wasSkinClaimed)
        {
            if (!CanPropose)
            {
                return;
            }
            
            if (wasSkinClaimed)
            {
                CurrentSkinProgress = 0f;
            }
            else 
            {
                float existProgress = CurrentSkinProgress;

                existProgress += SkinProgressDelta;
                existProgress = Mathf.Clamp01(existProgress);

                CurrentSkinProgress = existProgress;
            }
        }

        #endregion



        #region Events handlers

        private void ShooterSkins_OnOpened(ShooterSkinType type)
        {
            if (CurrentSkinTypeToPropose == type)
            {
                GenerateCurrentSkinTypeToPropose();
            }
        }


        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.Finished)
            {
                LevelContext context = levelEnvironment.Context;
                LevelProgress progress = levelEnvironment.Progress;


                bool isSceneMode = context.SceneMode.IsSceneMode();
                
                bool canChangeSkinProgress = context.IsEndOfLevel;
                canChangeSkinProgress &= !isSceneMode;
                canChangeSkinProgress &= progress.LevelResultState != LevelResult.Reload;

                if (canChangeSkinProgress)
                {
                    bool wasSkinClaimed = progress.WasSkinClaimed;
                    bool wasSkipped = progress.WasSkinSkipped;

                    LevelComplete(wasSkinClaimed || wasSkipped);
                }
            }
        }

        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Proposal.ProgresShooterSkinShowsCount);
            set => CustomPlayerPrefs.SetInt(PrefsKeys.Proposal.ProgresShooterSkinShowsCount, value);
        }
        
        #endregion
    }
}

