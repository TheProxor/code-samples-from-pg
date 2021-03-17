using System;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;
using Drawmasters.Interfaces;
using Drawmasters.AbTesting;
using Drawmasters.Ui.Mansion;
using Drawmasters.Levels;
using Drawmasters.Ui;


namespace Drawmasters.Proposal
{
    public class MansionProposeController : IProposalController
    {
        #region Fields

        public event Action OnStarted;
        public event Action OnFinished;

        #endregion



        #region Properties

        public static IUaAbTestMechanic uaAbTestMechanic =
            new CommonMechanicAvailability(PrefsKeys.Dev.DevMansionEnabled);

        public bool IsMechanicAvailable =>
            uaAbTestMechanic.WasAvailabilityChanged ?
            uaAbTestMechanic.IsMechanicAvailable : GameServices.Instance.AbTestService.CommonData.isMansionAvailable;


        public bool IsEnoughLevelsFinished =>
            GameServices.Instance.CommonStatisticService.LevelsFinishedCount >= GameServices.Instance.AbTestService.CommonData.minLevelForMansion;


        public bool CanForcePropose =>
            IsMechanicAvailable &&
            !CustomPlayerPrefs.HasKey(PrefsKeys.Tutorial.MansionForceShow) &&
            IsEnoughLevelsFinished;


        public bool IsActive =>
            IsMechanicAvailable &&
            IsEnoughLevelsFinished;


        public bool WasFirstLiveOpsStarted =>
            CustomPlayerPrefs.HasKey(PrefsKeys.Tutorial.MansionForceShow);


        public string TimeUi =>
            string.Empty;


        public bool ShouldShowAlert
        {
            get
            {
                MansionRewardController mansionRewardController = GameServices.Instance.ProposalService.MansionRewardController;
                IPlayerStatisticService playerStatisticsService = GameServices.Instance.PlayerStatisticService;

                bool canAnyUpgrade = default;
                bool isAnyPassiveRewardAvailable = default;

                for (int i = 0; i < PlayerMansionData.MansionRoomsCount; i++)
                {
                    canAnyUpgrade |= playerStatisticsService.PlayerMansionData.WasRoomOpened(i) &&
                                     !playerStatisticsService.PlayerMansionData.WasRoomCompleted(i);

                    isAnyPassiveRewardAvailable |= !mansionRewardController.IsTimerActive(i) &&
                                                   playerStatisticsService.PlayerMansionData.WasRoomCompleted(i);
                }

                canAnyUpgrade &= playerStatisticsService.CurrencyData.GetEarnedCurrency(CurrencyType.MansionHammers) > 0;

                return IsMechanicAvailable &&
                       IsEnoughLevelsFinished &&
                       (canAnyUpgrade || isAnyPassiveRewardAvailable);
            }
        }

        #endregion



        #region Methods

        public void AttemptStartProposal() { }


        public void MarkForceProposed() =>
            CustomPlayerPrefs.SetBool(PrefsKeys.Tutorial.MansionForceShow, true);
        

        public void Propose(Action<UiMansion> onShow = default)
        {
            if (IsEnoughLevelsFinished)
            {
                FromLevelToLevel.PlayTransition(() =>
                {
                    UiScreenManager.Instance.HideScreenImmediately(ScreenType.MainMenu);
                    LevelsManager.Instance.UnloadLevel();
                    UiMansion screen = UiScreenManager.Instance.ShowScreen(ScreenType.Mansion, isForceHideIfExist: true) as UiMansion;
                    screen.MarkMenuEnter();

                    onShow?.Invoke(screen);
                });
            }
            else
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.MansionMenu);
            }
        }

        #endregion
    }
}
