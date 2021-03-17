using System;
using UnityEngine;
using Drawmasters.Proposal;


namespace Drawmasters.Levels
{
    [Serializable]
    public class BonusLevelObjectData
    {
        public int stageIndex = default;
        
        public Vector2 linearVelocity = default;
        public float angularVelocity = default;
        public float acceleration = default;

        public RewardType rewardType = default;

        public CurrencyType currencyType = default;
        public float currencyAmount = default;

        public PetSkinType petSkinType = default;
    }
}

