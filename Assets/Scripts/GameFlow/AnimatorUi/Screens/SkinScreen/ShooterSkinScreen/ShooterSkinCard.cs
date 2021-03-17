using Drawmasters.OffersSystem;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class ShooterSkinCard : ChoosableCard<ShooterSkinType>
    {
        #region Properties

        public override bool IsActive =>
            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin == currentType;

        #endregion



        #region Methods

        protected override bool IsBought(ShooterSkinType type) =>
            GameServices.Instance.ShopService.ShooterSkins.IsBought(type);


        protected override Sprite GetIconSprite(ShooterSkinType type) =>
             IngameData.Settings.shooterSkinsSettings.GetSkinUiSprite(type);


        protected override void OnChooseCard() =>
            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin = currentType;
        
        
        protected override void PurchaseSubscriptionButton_OnClick() =>
            GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>().ForcePropose(OfferKeys.EntryPoint.Outfits);
        
        #endregion
    }
}

