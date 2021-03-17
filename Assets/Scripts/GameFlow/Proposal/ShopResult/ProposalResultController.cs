using System;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;


namespace Drawmasters.Proposal
{
    public abstract class ProposalResultController : ModesRewardController, IShowsCount
    {
        #region Fields

        private readonly SingleRewardPackSettings settings;

        #endregion



        #region Properties

        private int MinLevelForPropose => IsUaMinLevelDataSetted ?
            UaMinLevel : AbMinLevelForShopResultProposeKey;

        protected abstract ScreenType ScreenType { get; }

        protected abstract string ShowsCountKey { get; }

        protected abstract string UaShopResultMinLevelKey { get; }

        protected abstract int AbMinLevelForShopResultProposeKey { get; }

        #endregion



        #region Ua/Ab Properties

        public int UaMinLevel
        {
            get => CustomPlayerPrefs.GetInt(UaShopResultMinLevelKey, AbMinLevelForShopResultProposeKey);
            set => CustomPlayerPrefs.SetInt(UaShopResultMinLevelKey, value);
        }

        private bool IsUaMinLevelDataSetted => CustomPlayerPrefs.HasKey(UaShopResultMinLevelKey);

        #endregion



        #region Class lifecycle

        public ProposalResultController(SingleRewardPackSettings _settings,
                                    string _levelsCounterKey,
                                    string uaAllowKey,
                                    string _uaDeltaLevelsKey,
                                    IAbTestService abTestService,
                                    ILevelEnvironment levelEnvironment)
            : base(_levelsCounterKey,
                  uaAllowKey,
                  _uaDeltaLevelsKey,
                  abTestService,
                  levelEnvironment)
        {
            settings = _settings;
        }

        #endregion



        #region Methods

        public void Propose(GameMode mode, Action hidedCallback)
        {
            if (IsAvailable(mode))
            {
                InstantPropose(hidedCallback);
                MarkProposed(mode);
            }
            else
            {
                hidedCallback?.Invoke();
            }
        }


        public void InstantPropose(Action hidedCallback)
        {
            int showIndex = -1;
            if (!levelEnvironment.Context.Mode.IsHitmastersLiveOps())
            {
                showIndex = ShowsCount;
                ShowsCount++;
            }
            
            AnimatorScreen view = UiScreenManager.Instance.ShowScreen(ScreenType,
                                                  null,
                                                  hidedView => hidedCallback?.Invoke());

            if (view is IProposalRewardScreen proposalRewardScreen)
            {
                RewardData[] reward = settings.GetRewardPack(showIndex);
                proposalRewardScreen.SetReward(reward);
            }
        }


        public override bool IsAvailable(GameMode mode) =>
            base.IsAvailable(mode) && IsMinLevelReached(mode);


        public bool IsMinLevelReached(GameMode mode) => mode.GetFinishedLevels() >= MinLevelForPropose;


        public void SetupAvailableWeaponType(WeaponType weaponType) =>
            settings.SetupWeaponType(weaponType);

        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(ShowsCountKey);
            set => CustomPlayerPrefs.SetInt(ShowsCountKey, value);
        }

        #endregion



        #region Ua API

        public void ClearUaMinLevelData() => CustomPlayerPrefs.DeleteKey(UaShopResultMinLevelKey);

        #endregion
    }
}
