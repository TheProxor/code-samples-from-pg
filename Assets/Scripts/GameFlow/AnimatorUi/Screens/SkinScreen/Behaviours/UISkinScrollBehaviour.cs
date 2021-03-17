using System;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;


namespace Drawmasters.Ui
{
    public interface UISkinScrollBehaviour : IUiBehaviour
    {
        event Action<RewardData> OnShouldReceiveReward;
        
        #region properties
        
        IProposable Proposal { get; }
        string VideoPlacementKey { get; }
        UiPanelRewardController Controller { get; }
        string AnimatorTrigerKey { get; }
        
        #endregion
        
        
        
        #region Methods
        
        void Clear();
        
        #endregion
    }
}