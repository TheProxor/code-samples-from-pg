namespace Drawmasters.Proposal.Interfaces
{
    public interface IAlertable
    {
        bool CanShowAlert { get; }

        void OnProposalWasShown();
    }
}