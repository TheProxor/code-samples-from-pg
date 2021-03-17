using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters
{
    public class PetSkinsShop : IngameItemsShop<PetSkinType>
    {
        #region Fields

        public static PetSkinType[] UserSkinTypes =>
            Enum.GetValues(typeof(PetSkinType)).OfType<PetSkinType>().ToArray();

        protected override List<PetSkinType> InitialBoughtSkins => new List<PetSkinType> { PetSkinType.None };

        #endregion



        #region Class lifecycle

        public PetSkinsShop(string _savedKey,
                               IPlayerStatisticService playerStatistic)
            : base(_savedKey,
                   playerStatistic)
        { }

        #endregion



        #region Methods

        protected override float GetItemPrice(PetSkinType type)
        {
            CustomDebug.LogError("No logic implemented for pets prices");
            return default;
        }

        #endregion
    }
}
