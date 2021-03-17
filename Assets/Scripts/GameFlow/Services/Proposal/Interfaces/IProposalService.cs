using Drawmasters.OffersSystem;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Statistics;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IProposalService
    {
        ResultRewardController IngameCurrencyMultiplier { get; }

        RouletteRewardController RouletteRewardController { get; }

        ShopResultController ShopResultController { get; }

        PremiumShopResultController PremiumShopResultController { get; }

        RateUsProposal RateUsProposal { get; }

        SpinRouletteController SpinRouletteController { get; }

        ForceMeterController ForceMeterController { get; }

        IProposable LevelSkipProposal { get; }
        
        IProposable VideoShooterSkinProposal { get; }

        IProposable VideoWeaponSkinProposal { get; }

        CurrencyShopProposal CurrencyShopProposal { get; }

        SkinProposalStatistic SkinProposal { get; }

        MansionProposeController MansionProposeController { get; }

        MansionRewardController MansionRewardController { get; }

        UiPanelRewardController UiPanelShooterSkinRewardController { get; }

        UiPanelRewardController UiPanelWeaponSkinRewardController { get; }

        MonopolyProposeController MonopolyProposeController { get; }
        
        HitmastersProposeController HitmastersProposeController { get; }

        SeasonEventProposeController SeasonEventProposeController { get; }

        LeagueProposeController LeagueProposeController { get; }

        HappyHoursLeagueProposeController HappyHoursLeagueProposeController { get; }

        HappyHoursSeasonEventProposeController HappyHoursSeasonEventProposeController { get; }

        T GetOffer<T>() where T : BaseOffer;

        T[] GetOffers<T>() where T : class;
    }
}

