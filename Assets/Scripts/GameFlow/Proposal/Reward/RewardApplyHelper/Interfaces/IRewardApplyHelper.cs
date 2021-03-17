using System;
using Drawmasters;
using Drawmasters.Proposal;

namespace Drawmasters.Interfaces
{
    public interface IRewardApplyHelper : IDeinitializable
    {
        void ApplyReward(RewardData reward, Action onClaimed);
    }
}