using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Proposal
{
    public class HitmastersProposals
    {
        #region Helpers

        public class Proposal
        {
            public Func<GameMode, int, bool> IsAvailable { get; set; }
            public Action<GameMode> LoadAction { get; set; }
        }

        #endregion



        #region Fields

        private readonly IAbTestService abTestService;
        private readonly List<Proposal> proposals;

        #endregion



        #region Ctor

        public HitmastersProposals(IAbTestService _abTestService, ProposalsLoader _proposalsLoader)
        {
            abTestService = _abTestService;

            proposals = new List<Proposal>()
            {
                new Proposal()
                {
                    IsAvailable = IsPremiumShopAvailable,
                    LoadAction = _proposalsLoader.PremiumShopInstantShow
                },
                new Proposal()
                {
                    IsAvailable = IsRouletteAvailable,
                    LoadAction = _proposalsLoader.RouletteInstantShow
                },
                new Proposal()
                {
                    IsAvailable = IsForceMeterAvailable,
                    LoadAction = _proposalsLoader.ForcemeterInstantShow
                }
            };
        }

        #endregion


        
        #region Methods

        public bool IsAnyProposalAvailable(GameMode mode, 
            int currentLevelIndex, 
            out Proposal availableProposal)
        {
            availableProposal = proposals.Find(e => e.IsAvailable(mode, currentLevelIndex));

            return availableProposal != null;
        }


        public void LoadProposal(GameMode mode, int currentLevelIndex)
        {
            if (!IsAnyProposalAvailable(mode, currentLevelIndex, out Proposal availableProposal))
            {
                CustomDebug.Log($"Proposal for mode {mode} and index {currentLevelIndex} is not available");
                return;
            }
            
            availableProposal.LoadAction?.Invoke(mode);
        }
        

        public bool IsPremiumShopAvailable(GameMode mode, int currentLevelIndex)
        {
            int[] allowedIndexes = abTestService.CommonData.hitmastersSpinOffPremiumShopIndexes;
            
            return allowedIndexes.Contains(e => e == currentLevelIndex);
        }

        public bool IsRouletteAvailable(GameMode mode, int currentLevelIndex)
        {
            int[] allowedIndexes = abTestService.CommonData.hitmastersSpinOffRouletteIndexes;
            
            return allowedIndexes.Contains(e => e == currentLevelIndex);
        }
        
        public bool IsForceMeterAvailable(GameMode mode, int currentLevelIndex)
        {
            int[] allowedIndexes = abTestService.CommonData.hitmastersSpinOffForcemeterIndexes;
            return allowedIndexes.Contains(e => e == currentLevelIndex);
        }

        public bool IsBossAvailable(GameMode mode, int currentLevelIndex)
        {
            int[] allowedIndexes = abTestService.CommonData.hitmastersSpinOffBossIndexes;
            
            return allowedIndexes.Contains(e => e == currentLevelIndex);
        }

        #endregion
    }
}
