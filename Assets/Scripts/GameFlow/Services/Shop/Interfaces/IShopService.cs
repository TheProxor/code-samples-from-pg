using Modules.InAppPurchase;

namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IShopService
    {
        ShooterSkinsShop ShooterSkins { get; }

        WeaponSkinsShop WeaponSkins { get; }

        PetSkinsShop PetSkins { get; }

        IAPs IAPs { get; }
    }
}

