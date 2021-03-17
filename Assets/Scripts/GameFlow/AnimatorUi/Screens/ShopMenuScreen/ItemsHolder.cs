using System.Collections.Generic;


namespace Drawmasters.Ui
{
    public class ItemsHolder : IInitializable, IDeinitializable
    {
        #region Fields

        public readonly List<IShopMenuCell> cells;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            foreach(var i in cells)
            {
                i.OnBecomeUnavailable += ShopMenuCellOnBecomeUnavailable;
            }
        }        

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            foreach(var i in cells)
            {
                i.OnBecomeUnavailable -= ShopMenuCellOnBecomeUnavailable;
                i.Deinitialize();
            }
        }

        #endregion



        #region Ctor

        public ItemsHolder(params UiIAPScreen.CellPair[] items)
        {
            cells = new List<IShopMenuCell>();

            foreach (var i in items)
            {
                i.cell.Initialize(i.storeItem);

                cells.Add(i.cell);
            }
        }

        #endregion



        #region Methods

        public void UpdateFxOrder(int order)
        {
            foreach (var i in cells)
            {
                i.UpdateFxSortingOrder(order);
            }
        }


        private void RemoveCell(IShopMenuCell cell)
        {
            cell.OnBecomeUnavailable -= ShopMenuCellOnBecomeUnavailable;
            cell.Deinitialize();
            
            cells.Remove(cell);
        }

        #endregion



        #region Events handlers

        private void ShopMenuCellOnBecomeUnavailable(IShopMenuCell shopMenuCell) =>
            RemoveCell(shopMenuCell);
        
        #endregion
    }
}
