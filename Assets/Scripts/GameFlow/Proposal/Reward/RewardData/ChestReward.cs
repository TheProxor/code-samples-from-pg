using System;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class ChestReward : RewardData
    {
        #region Fields

        public ChestType chestType = ChestType.Wooden;

        private RewardData[] containedRewards;

        #endregion



        #region Properties

        public override RewardType Type =>
            RewardType.Chest;


        public string UiRewardText =>
            chestType.ToString();

        public override bool IsAvailableForRewardPack =>
            true;

        public RewardData[] ContainedRewards =>
            containedRewards;
        
        private ChestData ChestData =>
            IngameData.Settings.league.chestSettings.GetChestData(chestType);

        #endregion



        #region Class lifecycle

        public ChestReward()
        {
        }

        #endregion



        #region Methods

        public override void Open()
        {
            containedRewards = ChestData.RandomChestRewards;

            foreach (var r in containedRewards)
            {
                r.Open();
            }
        }


        public override void Apply()
        {
            base.Apply();

            if (containedRewards == null)
            {
                containedRewards = ChestData.RandomChestRewards;
                CustomDebug.Log($"Attemp to apply rewards in {this} without open. Filled random rewards");
            }

            foreach (var r in containedRewards)
            {
                r.Apply();
            }
        }


        public override Sprite GetUiSprite() =>
            default;
            

        public override bool IsEqualsReward(RewardData anotherReward)
        {
            bool result = base.IsEqualsReward(anotherReward);

            if (anotherReward is ChestReward chestReward)
            {
                result = chestType == chestReward.chestType;
            }

            return result;
        }

        #endregion
    }
}
