using System;
using Modules.General.InAppPurchase;


namespace Drawmasters.Ui
{
    public interface IShopMenuCell : IDeinitializable
    {
        event Action<IShopMenuCell> OnBecomeUnavailable;

        void Initialize(StoreItem _storeItem);
        void UpdateFxSortingOrder(int order);
    }
}
