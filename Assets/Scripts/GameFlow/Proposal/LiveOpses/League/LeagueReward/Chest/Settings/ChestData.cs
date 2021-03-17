using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal.Settings;
using Drawmasters.Statistics.Data;
using Sirenix.OdinInspector;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class ChestData
    {
        #region Fields

        public ChestType chestType = default;

        public ChestRewardType chestRewardMask = default;

        [ShowIf("CanShowSoftCurrencyField")] 
        public List<CurrencyReward> softCurrencyRewards = default;

        [ShowIf("CanShowHardCurrencyField")] 
        public List<CurrencyReward> hardCurrencyRewards = default;

        [ShowIf("CanShowMansionHammersField")] 
        public List<CurrencyReward> mansionHammersCurrencyRewards = default;

        [ShowIf("CanShowBonesCurrencyField")]
        public List<CurrencyReward> bonesCurrencyRewards = default;

        [ShowIf("CanShowSkinField")]
        public ChestSkinsRewardSettings chestSkinsRewardData = default;

        #endregion


        #region Helper inspector properties

        private bool CanShowSoftCurrencyField => 
            chestRewardMask.HasFlag(ChestRewardType.SoftCurrency);

        private bool CanShowHardCurrencyField =>
            chestRewardMask.HasFlag(ChestRewardType.HardCurrency);

        private bool CanShowMansionHammersField =>
            chestRewardMask.HasFlag(ChestRewardType.MansionHammers);

        private bool CanShowBonesCurrencyField =>
            chestRewardMask.HasFlag(ChestRewardType.BonesCurrency);

        private bool CanShowSkinField =>
            chestRewardMask.HasFlag(ChestRewardType.ShooterSkin) ||
            chestRewardMask.HasFlag(ChestRewardType.PetSkin) ||
            chestRewardMask.HasFlag(ChestRewardType.WeaponSKin);

        #endregion



        #region Properties

        public RewardData[] RandomChestRewards
        {
            get
            {
                List<RewardData> result = new List<RewardData>();

                LeagueRewardSettings settings = IngameData.Settings.league.leagueRewardSettings;
                
                List<RewardData> skinBuffer = new List<RewardData>();
                if (chestRewardMask.HasFlag(ChestRewardType.ShooterSkin))
                {
                    RewardData data = chestSkinsRewardData.RandomShooterSkinReward;
                    if (data != null)
                    {
                        skinBuffer.Add(data);
                    }
                }
                
                if (chestRewardMask.HasFlag(ChestRewardType.WeaponSKin))
                {
                    RewardData data = chestSkinsRewardData.RandomWeaponSkinReward;
                    if (data != null)
                    {
                        skinBuffer.Add(data);
                    }
                }

                if (chestRewardMask.HasFlag(ChestRewardType.PetSkin))
                {
                    RewardData data = chestSkinsRewardData.RandomPetSkinReward;
                    if (data != null)
                    {
                        skinBuffer.Add(data);
                    }
                }

                var mainReward = skinBuffer.RandomObject();
                if (mainReward == null)
                {
                    if (CanShowSkinField)
                    {
                        mainReward = settings.fallbackReward;
                    }
                }

                if (mainReward != null)
                {
                    result.Add(mainReward);
                }
                

                if (CanShowSoftCurrencyField)
                {
                    RewardData[] rewards = softCurrencyRewards.OfType<RewardData>().ToArray();

                    RewardData data = RewardDataUtility.GetRandomReward(rewards);
                    
                    result.Add(data);
                }

                if (chestRewardMask.HasFlag(ChestRewardType.HardCurrency))
                {
                    RewardData[] rewards = hardCurrencyRewards.OfType<RewardData>().ToArray();

                    RewardData data = RewardDataUtility.GetRandomReward(rewards);
                    
                    result.Add(data);
                }

                if (CanShowMansionHammersField &&
                    CurrencyType.MansionHammers.IsAvailableForShow())
                {
                    RewardData[] rewards = mansionHammersCurrencyRewards.OfType<RewardData>().ToArray();

                    RewardData data = RewardDataUtility.GetRandomReward(rewards);
                    
                    result.Add(data);
                }

                if (CanShowBonesCurrencyField &&
                    CurrencyType.RollBones.IsAvailableForShow())
                {
                    RewardData[] rewards = bonesCurrencyRewards.OfType<RewardData>().ToArray();

                    RewardData data = RewardDataUtility.GetRandomReward(rewards);
                    
                    result.Add(data);
                }

                return result.ToArray();
            }
        }

        #endregion
    }
}