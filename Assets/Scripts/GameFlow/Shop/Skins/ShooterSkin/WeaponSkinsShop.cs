using System.Collections.Generic;
using Drawmasters.ServiceUtil.Interfaces;

namespace Drawmasters
{
    public class WeaponSkinsShop : IngameItemsShop<WeaponSkinType>
    {
        #region Fields

        protected override List<WeaponSkinType> InitialBoughtSkins => new List<WeaponSkinType> { WeaponSkinType.BowDefault };

        #endregion



        #region Class lifecycle

        public WeaponSkinsShop(string _savedKey,
                               IPlayerStatisticService playerStatistic)
            : base(_savedKey,
                   playerStatistic)
        { }

        #endregion



        #region Methods

        protected override float GetItemPrice(WeaponSkinType type) => IngameData.Settings.weaponSkinSettings.FindPrice(type);

        #endregion
    }
}
