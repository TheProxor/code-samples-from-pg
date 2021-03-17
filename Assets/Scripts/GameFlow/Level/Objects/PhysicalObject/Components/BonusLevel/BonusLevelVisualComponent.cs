using Drawmasters.Statistics.Data;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class BonusLevelVisualComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] private List<(CurrencyType, Sprite)> sprites = default;

        #endregion



        #region Abstract implementation

        public override void Enable()
        {
            if (sourceLevelObject.CurrentData.bonusData.rewardType != Proposal.RewardType.Currency)
            {
                return;
            }

            CurrencyType type = sourceLevelObject.CurrentData.bonusData.currencyType;

            if (type.IsMansionCurrency() && !type.IsMansionAvailableForShow())
            {
                type = CurrencyType.Simple;
            }
            else if (type.IsMonopolyCurrency() && !type.IsMonopolyAvailableForShowOnLevel())
            {
                type = CurrencyType.Premium;
            }

            foreach (var i in sprites)
            {
                if (i.Item1 == type)
                {
                    sourceLevelObject.SpriteRenderer.sprite = i.Item2;
                    break;
                }
            }
        }

        public override void Disable()
        {

        }

        #endregion
    }
}
