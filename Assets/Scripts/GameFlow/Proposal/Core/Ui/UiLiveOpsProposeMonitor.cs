using System;
using System.Linq;
using System.Collections.Generic;


namespace Drawmasters.Proposal
{
    public class UiLiveOpsProposeMonitor : IInitializable, IDeinitializable
    {
        #region Fields

        public event Action OnShouldForcePropose;
        
        public event Action<IUiProposal> OnShouldAddLiveOpsForSwipeScreen;
        public event Action<IUiProposal> OnShouldSwipeToLiveOps;

        private readonly IEnumerable<IUiProposal> proposals;

        private static object lastSwipedUiLiveOpsController;

        #endregion



        #region Properties

        public IUiProposal LastSwipedUiLiveOpsProposal
        {
            get
            {
                IUiProposal result;

                if (lastSwipedUiLiveOpsController == null)
                {
                    IEnumerable<IUiProposal> swipeProposals = proposals.Where(e => e.CanStartSwipeWithProposal);
                    
                    var uiLiveOpsProposes = swipeProposals.ToList();
                    
                    result = uiLiveOpsProposes.Any() ? 
                        uiLiveOpsProposes.First() : 
                        proposals.FirstOrDefault();
                }
                else
                {
                    result = proposals.FirstOrDefault(e => e.Key == lastSwipedUiLiveOpsController);
                }

                return result;
            }
        }

        public bool IsAnyForceProposeActive { get; set; }

        #endregion



        #region Class lifecycle

        public UiLiveOpsProposeMonitor(params IUiProposal[] _proposals)
        {
            proposals = _proposals;
        }

        #endregion



        #region IInitializable

        public void Initialize()
        {
            UiProposal.OnShouldSetLockedProposals += UiProposal_OnShouldSetLockedProposals;
            UiProposal.OnShouldForcePropose += Proposal_OnShouldForcePropose;

            foreach (var proposal in proposals)
            {
                proposal.OnShouldAddProposal += Proposal_OnShouldAddPropose;
                proposal.OnShouldSwipeToProposal += Proposal_OnShouldSwipeToProposal;
            }
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            UiProposal.OnShouldSetLockedProposals -= UiProposal_OnShouldSetLockedProposals;
            UiProposal.OnShouldForcePropose -= Proposal_OnShouldForcePropose;

            foreach (var proposal in proposals)
            {
                proposal.OnShouldAddProposal -= Proposal_OnShouldAddPropose;
                proposal.OnShouldSwipeToProposal -= Proposal_OnShouldSwipeToProposal;
            }

            // HOTFIX: behaviour bug (deiniti+init sometimes) IsAnyForceProposeActive = false;
        }
        
        #endregion
        
        
        
        #region Public methods

        public void SetupLastSwipedUiLiveOpsProposal(IUiProposal target)
        {
            if (target != null)
            {
                lastSwipedUiLiveOpsController = target.Key;
            }
        }

        #endregion



        #region Events handlers

        private void UiProposal_OnShouldSetLockedProposals(bool enabled) =>
            IsAnyForceProposeActive = enabled;


        private void Proposal_OnShouldForcePropose() =>
            OnShouldForcePropose?.Invoke();
        

        private void Proposal_OnShouldAddPropose(IUiProposal uiLiveOps) =>
            OnShouldAddLiveOpsForSwipeScreen?.Invoke(uiLiveOps);


        private void Proposal_OnShouldSwipeToProposal(IUiProposal uiLiveOps) =>
            OnShouldSwipeToLiveOps?.Invoke(uiLiveOps);

        #endregion
    }
}
