using Drawmasters.Proposal.Interfaces;
using System;


namespace Drawmasters.Proposal
{
    public class NoVideoImplementation : IProposeImplementation
    {
        public void Propose(Action<bool> onProposed) => onProposed?.Invoke(true);
    }
}
