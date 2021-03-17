using System;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;
using Drawmasters.Ui;


namespace Drawmasters
{
    public class CommonSkinRewardApplyHelper : IRewardApplyHelper
    {
        protected readonly RewardReceiveScreen screen;
        
        public CommonSkinRewardApplyHelper(RewardReceiveScreen withScreen)
        {
            screen = withScreen;
        }



        #region IDeinitializable

        public void Deinitialize()
        {
            
        }

        #endregion

        #region IRewardApplyHelper

        public void ApplyReward(RewardData reward, Action onClaimed)
        {
            reward.Open();

            UiScreenManager.Instance.ShowScreen(ScreenType.SpinReward, onShowBegin: (view) =>
            {
                if (view is UiRewardReceiveScreen rewardScreen)
                {
                    rewardScreen.SetupFxKey(screen.RewardScreenIdleFxKey);
                    rewardScreen.SetupReward(reward);
                }
            }, onHided: (hidedView) => onClaimed?.Invoke());
        }

        #endregion
    }
}