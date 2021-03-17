using Drawmasters.Advertising;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Ui;
using System;
using Modules.Advertising;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General.Abstraction;
using IAbTestService = Drawmasters.ServiceUtil.Interfaces.IAbTestService;


namespace Drawmasters.Proposal
{
    public class VideoShooterSkin : IProposable, IAlertable
    {
        #region Fields

        public readonly AdsSkinPanelsSettings proposalSettings;

        protected readonly IAbTestService abTestService;

        protected readonly ICommonStatisticsService commonStatisticService;

        #endregion



        #region Properties

        protected virtual bool IsMechanicsAvailable =>
            abTestService.CommonData.isVideoShooterSkinProposalAvailable;

        protected virtual bool IsMinLevelReached =>
            commonStatisticService.LevelsFinishedCount >= abTestService.CommonData.videoShooterSkinProposalMinLevel;

        protected virtual string VideoPlacement =>
            AdsVideoPlaceKeys.ShooterSkinsPanel;

        protected int SuccessfulProposeCount
        {
            get => CustomPlayerPrefs.GetInt(ProposeCountKey);
            set => CustomPlayerPrefs.SetInt(ProposeCountKey, value);
        }

        protected bool WasShownProposal
        {
            get => CustomPlayerPrefs.GetBool(WasShownKey);
            set => CustomPlayerPrefs.SetBool(WasShownKey, value);
        }

        protected virtual string ProposeCountKey => PrefsKeys.Proposal.VideoShooterSkinProposeCountKey;
        protected virtual string WasShownKey => PrefsKeys.Proposal.VideoShooterSkinWasShownKey;

        #endregion



        #region IAlertable

        public bool CanShowAlert => 
            SuccessfulProposeCount > 0 && 
            CanPropose &&
            !WasShownProposal;

        public void OnProposalWasShown() =>
            WasShownProposal = true;

        #endregion



        #region IProposable

        public bool CanPropose
        {
            get
            {
                bool canPropose = true;

                canPropose &= IsAvailable;
                canPropose &= proposalSettings.IsCoolDownExpired;

                return canPropose;
            }
        }


        public bool IsAvailable
        {
            get
            {
                bool isAvailable = true;

                bool isExistProposingContent = proposalSettings.IsAnyCommonRewardAvailable;

                isAvailable &= IsMechanicsAvailable;
                isAvailable &= IsMinLevelReached;
                isAvailable &= isExistProposingContent;

                return isAvailable;
            }
        }

        public void Propose(Action<bool> onProposed)
        {
            SuccessfulProposeCount++;

            WasShownProposal = false;
        }

        #endregion



        #region Ctor

        public VideoShooterSkin(AdsSkinPanelsSettings _settings,
                                IAbTestService _abTestService,
                                ICommonStatisticsService _commonStatisticsService)
        {
            proposalSettings = _settings;

            abTestService = _abTestService;
            commonStatisticService = _commonStatisticsService;
        }

        #endregion
    }
}
