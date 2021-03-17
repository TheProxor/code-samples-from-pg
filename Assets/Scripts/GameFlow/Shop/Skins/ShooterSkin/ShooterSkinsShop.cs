using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters
{
    public class ShooterSkinsShop : IngameItemsShop<ShooterSkinType>
    {
        #region Fields

        protected override List<ShooterSkinType> InitialBoughtSkins => new List<ShooterSkinType> { ShooterSkinType.Archer };

        #endregion



        #region Class lifecycle

        public ShooterSkinsShop(string _savedKey,
                                IPlayerStatisticService playerStatistic) :
            base(_savedKey,
                 playerStatistic)
        { }

        #endregion



        #region Methods

        protected override float GetItemPrice(ShooterSkinType type) => throw new NotImplementedException("No logic for price");
        
        #endregion
    }
}
