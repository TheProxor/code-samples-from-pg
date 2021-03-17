using Drawmasters.Interfaces;


namespace Drawmasters.Proposal.Interfaces
{
    public interface IProposable : IProposeImplementation, IAvailable
    {
        bool CanPropose { get; }
    }
}
