namespace Drawmasters.Proposal.Interfaces
{
    public interface IForceProposal
    {
        bool CanForcePropose { get; }

        void MarkForceProposed();
    }
}
