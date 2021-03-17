using System;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class CurrencySkinsIconData
    {
        public CurrencyType type = default;

        public Sprite activeIcon = default;
        public Sprite claimedIcon = default;
    }
}