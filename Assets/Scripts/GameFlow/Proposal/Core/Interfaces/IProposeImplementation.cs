using System;


namespace Drawmasters.Proposal.Interfaces
{
    public interface IProposeImplementation
    {
        void Propose(Action<bool> onProposed); // bool - is proposed successful
    }
}
