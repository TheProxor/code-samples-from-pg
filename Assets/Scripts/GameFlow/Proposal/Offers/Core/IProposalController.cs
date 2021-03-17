using System;
using Drawmasters.Proposal.Interfaces;


namespace Drawmasters.Proposal
{
    public interface IProposalController : IForceProposal
    {
        event Action OnStarted;
        event Action OnFinished;

        bool IsMechanicAvailable { get; }

        bool IsEnoughLevelsFinished { get; }

        bool IsActive { get; }

        bool WasFirstLiveOpsStarted { get; }

        bool ShouldShowAlert { get; }

        string TimeUi { get; }

        void AttemptStartProposal();
    }
}
