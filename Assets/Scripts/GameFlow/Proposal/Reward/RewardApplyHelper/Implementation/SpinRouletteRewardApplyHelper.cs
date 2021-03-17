using System;
using Drawmasters;
using Drawmasters.Interfaces;
using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.Ui;


namespace Drawmasters
{
    public class SpinRouletteRewardApplyHelper<T> : IRewardApplyHelper where T : SpinRouletteReward
    {
        #region Fields
        
        protected readonly SpinRouletteSettings settings;
        
        #endregion



        #region Ctor

        public SpinRouletteRewardApplyHelper(SpinRouletteSettings _settings)
        {
            settings = _settings;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize() { }

        #endregion



        #region IRewardApplyHelper

        public void ApplyReward(RewardData reward, Action onClaimed) 
        {
            if (settings == null)
            {
                CustomDebug.Log($"{settings.GetType().Name} is NULL.");
                onClaimed?.Invoke();
                
                return;
            }
            
            if (reward is T castReward)
            {
                castReward.SetupRewardPackSettings(settings);
                castReward.SetupHidedCallback(OnClaimed);
                castReward.Open();
                castReward.Apply();
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