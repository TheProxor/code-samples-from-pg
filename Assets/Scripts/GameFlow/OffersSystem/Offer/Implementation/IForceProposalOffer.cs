using System;


namespace Drawmasters.Proposal.Interfaces
{
    public interface IForceProposalOffer
    {
        bool CanForcePropose { get; }

        void MarkForceProposed();

        void ForcePropose(Action onProposeHidden = default);
        
        void ForcePropose(string entryPoint, Action onProposeHidden = default);
    }
}
