using System;
using System.Collections.Generic;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class CompleteLevelFinisher : SucceedLevelFinisher
    {
        #region Helpers

        private class Proposal
        {
            public Func<GameMode, bool> IsAvailable { get; set; }
            public Action<GameMode, Action> OnTransitionShown { get; set; }
        }

        #endregion



        #region Fields

        private readonly List<Proposal> proposals;

        #endregion



        #region Ctor

        public CompleteLevelFinisher(ProposalsLoader proposalsLoader)
        {
            proposals = new List<Proposal>()
            {
                new Proposal()
                {
                    IsAvailable = IsPremiumShopAvailable,
                    OnTransitionShown = proposalsLoader.PremiumShopShow
                },
                new Proposal()
                {
                    IsAvailable = IsShopResultAvailable,
                    OnTransitionShown = proposalsLoader.ShopResultShow
                },
                new Proposal()
                {
                    IsAvailable = IsRouletteAvailable,
                    OnTransitionShown = proposalsLoader.RouletteShow
                },
                new Proposal()
                {
                    IsAvailable = IsForceMeterAvailable,
                    OnTransitionShown = proposalsLoader.ForceMeterShow
                }
            };
        }

        #endregion



        #region Overrided methods

        public override void FinishLevel(Action _onFinished)
        {
            base.FinishLevel(_onFinished);

            LevelContext context = levelEnvironment.Context;
            LevelProgress progress = levelEnvironment.Progress;

            GameMode mode = context.Mode;

            bool canProposalShow = true;
            canProposalShow &= progress.CanShowPropose;
            canProposalShow &= mode != GameMode.UaLandscapeMode;
            canProposalShow &= !context.IsBossLevel;
            canProposalShow &= !context.IsBonusLevel;

            if (canProposalShow)
            {
                Proposal proposal = default;

                foreach(var i in proposals)
                {
                    if (i.IsAvailable(mode))
                    {
                        proposal = i;
                    }
                }

                if (proposal != null)
                {
                    //HACK
                    if (progress.IsLevelSkipped)
                    {
                        UiScreenManager.Instance.HideScreen(ScreenType.Ingame);
                    }
                    else
                    {
                        UiScreenManager.Instance.HideAll();
                    }

                    FromLevelToLevel.PlayTransition(() =>
                    {
                        proposal.OnTransitionShown?.Invoke(mode, onFinished);    
                    });
                }
                else
                {
                    ShowResult();
                }
            }
            else
            {
                ShowResult();
            }
        }

        #endregion



        #region Proposals implementation

        private bool IsPremiumShopAvailable(GameMode mode) =>
            proposalService.PremiumShopResultController.IsAvailable(mode);

        
        private bool IsShopResultAvailable(GameMode mode) =>
            proposalService.ShopResultController.IsAvailable(mode);


        private bool IsRouletteAvailable(GameMode mode) =>
            proposalService.RouletteRewardController.IsAvailable(mode);


        private bool IsForceMeterAvailable(GameMode mode) =>
            proposalService.ForceMeterController.IsAvailable(mode);

        #endregion
    }
}