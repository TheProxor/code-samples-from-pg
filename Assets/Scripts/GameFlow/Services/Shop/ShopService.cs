using Drawmasters.ServiceUtil.Interfaces;
using Modules.InAppPurchase;

namespace Drawmasters.ServiceUtil
{
    public class ShopService : IShopService
    {
        #region IShopService

        public ShooterSkinsShop ShooterSkins { get; }

        public WeaponSkinsShop WeaponSkins { get; }

        public PetSkinsShop PetSkins { get; }
        
        public IAPs IAPs { get; }

        #endregion



        #region Ctor

        public ShopService(IPlayerStatisticService playerStatistic)
        {
            IAPs = new IAPs();
            ShooterSkins = new ShooterSkinsShop(PrefsKeys.Shop.BoughtShooterSkinsUpdate,
                                                playerStatistic);
            WeaponSkins = new WeaponSkinsShop(PrefsKeys.Shop.BoughtWeaponSkinsUpdate,
                                              playerStatistic);
            PetSkins = new PetSkinsShop(PrefsKeys.Shop.BoughtPetSkinsUpdate,
                                              playerStatistic);
        }

        #endregion
    }
}

