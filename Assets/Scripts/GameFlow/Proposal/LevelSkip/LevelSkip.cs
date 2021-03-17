using System;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Proposal
{
    public class LevelSkip : IProposable
    {
        #region Properties

        private bool NeedRewardVideoWatching => GameServices.Instance.AbTestService.CommonData.isSkipLevelNeedRewardVideo;

        #endregion



        #region IProposable

        public bool CanPropose => IsAvailable;

        public bool IsAvailable => GameServices.Instance.AbTestService.CommonData.isSkipLevelProposalEnabled;

        public void Propose(Action<bool> onProposed)
        {
            IProposeImplementation implementation = null;
            
            if (NeedRewardVideoWatching)
            {
                implementation = new RewardVideoImplementation();
            }
            else
            {
                implementation = new NoVideoImplementation();
            }

            if (CanPropose)
            {
                implementation.Propose(onProposed);
            }
            else
            {
                CustomDebug.Log("Wrong proposal logic. You should check proposal availability before.");

                onProposed?.Invoke(false);
            }
        }

        #endregion
    }
}
