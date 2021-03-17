using Modules.General.InAppPurchase;

namespace Drawmasters
{
    public static class StoreItemExtension
    {
        #region Public methods

        public static bool IsPurchased(this StoreItem item)
        {
            bool result = false;

            if (item != null)
            {
                result = StoreManager.Instance.IsStoreItemPurchased(item);
            }
            
            return result;
        }

        #endregion
    }
}
