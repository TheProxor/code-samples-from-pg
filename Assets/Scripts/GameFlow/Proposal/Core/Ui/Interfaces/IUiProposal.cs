using System;
using Drawmasters.Ui;


namespace Drawmasters.Proposal
{
    public interface IUiProposal : IUiSwipesRect, IInitializable, IDeinitializable
    {
        event Action<IUiProposal> OnShouldAddProposal;
        event Action<IUiProposal> OnShouldSwipeToProposal;

        bool CanStartSwipeWithProposal { get; }
        bool ShouldShowProposalRoot { get; }
        object Key { get; }
        bool CanForcePropose { get; }

        void ForceProposeWithMenuShow();
        void InitializeButtons();
        void DeinitializeButtons();
    }
}
