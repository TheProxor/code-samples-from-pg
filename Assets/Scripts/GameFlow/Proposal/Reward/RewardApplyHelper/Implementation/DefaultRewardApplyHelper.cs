using System;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;


namespace Drawmasters
{
    public class DefaultRewardApplyHelper : IRewardApplyHelper
    {
        public void Deinitialize(){ }

        public void ApplyReward(RewardData reward, Action onClaimed)
        {
            reward.Open();
            reward.Apply();

            onClaimed?.Invoke();
        }
    }
}