using System;
using Drawmasters.Interfaces;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.Ui;


namespace Drawmasters
{
    public class ForcemeterRewardApplyHelper : IRewardApplyHelper
    {
        #region Fields

        protected readonly SequenceRewardPackSettings settings;

        #endregion



        #region Ctor

        public ForcemeterRewardApplyHelper(SequenceRewardPackSettings _settings)
        {
            settings = _settings;
        }

        #endregion



        #region IRewardApplyHelper

        public void Deinitialize() { }

        public void ApplyReward(RewardData reward, Action onClaimed)
        {
            if (reward is ForcemeterReward forcemeterReward)
            {
                forcemeterReward.SetupRewardPackSettings(settings);
                forcemeterReward.SetupHidedCallback(OnClaimed);
                forcemeterReward.Open();
                forcemeterReward.Apply();
            }
        }

        #endregion
        
        
        
        #region Protected methods

        protected void OnClaimed()
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                LevelsManager.Instance.UnloadLevel();
                UiScreenManager.Instance.HideAll(true);

                UiScreenManager.Instance.ShowScreen(ScreenType.SeasonEventScreen, isForceHideIfExist: true);
            });
        }

        #endregion
    }
}