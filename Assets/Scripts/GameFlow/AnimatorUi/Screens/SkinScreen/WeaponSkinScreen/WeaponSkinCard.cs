using Drawmasters.OffersSystem;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class WeaponSkinCard : ChoosableCard<WeaponSkinType>
    {
        #region Fields

        private WeaponType weaponType;

        #endregion



        #region Properties

        public override bool IsActive =>
            GameServices.Instance.PlayerStatisticService.PlayerData.GetCurrentWeaponSkin(weaponType) == currentType;

        #endregion



        #region Methods

        public void SetupWeaponType(WeaponType _weaponType) =>
            weaponType = _weaponType;


        protected override bool IsBought(WeaponSkinType type) =>
            GameServices.Instance.ShopService.WeaponSkins.IsBought(type);


        protected override Sprite GetIconSprite(WeaponSkinType type) =>
            IngameData.Settings.weaponSkinSettings.GetSkinUiSprite(type);
        

        protected override void OnChooseCard() =>
            GameServices.Instance.PlayerStatisticService.PlayerData.SetWeaponSkin(weaponType, currentType);

        
        protected override void PurchaseSubscriptionButton_OnClick() =>
            GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>().ForcePropose(OfferKeys.EntryPoint.Guns);
        
        #endregion
    }
}
