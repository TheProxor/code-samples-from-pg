using Drawmasters.Advertising;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    // TODO wrong inheritance
    class VideoWeaponSkin : VideoShooterSkin
    {
        #region Overrided properties

        protected override bool IsMechanicsAvailable => abTestService.CommonData.isVideoWeaponSkinProposalAvailable;

        protected override bool IsMinLevelReached 
            => commonStatisticService.LevelsFinishedCount >=  abTestService.CommonData.videoWeaponSkinProposalMinLevel;

        protected override string VideoPlacement => AdsVideoPlaceKeys.WeaponSkinsPanel;

        protected override string ProposeCountKey => PrefsKeys.Proposal.VideoWeaponSkinProposeCountKey;
        protected override string WasShownKey => PrefsKeys.Proposal.VideoWeaponSkinWasShownKey;

        #endregion



        #region Ctor

        
        public VideoWeaponSkin(AdsSkinPanelsSettings _settings,
                               IAbTestService _abTestService,
                               ICommonStatisticsService _commonStatisticsService)
            : base(_settings, _abTestService, _commonStatisticsService) { }

        #endregion
    }
}
