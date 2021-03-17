using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CurrencySpritesSettings = Drawmasters.Proposal.CommonRewardSettings.CurrencySpritesSettings;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "ForceMeterRewardPackSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "ForceMeterRewardPackSettings")]
    public partial class ForceMeterRewardPackSettings : SequenceForcemeterRewardPackSettings
    {
        #region Fields

        [SerializeField] private SegmentData[] segmentData = default;

        [Header("Visuals")]
        [SerializeField] private CurrencySpritesSettings[] currencySpritesSettings = default;

        #endregion



        #region Methods
        
        public Sprite GetWeaponSkinSprite(WeaponSkinType type) =>
            IngameData.Settings.weaponSkinSettings.GetUiOutlineSprite(type);


        public Sprite GetShooterSkinSprite(ShooterSkinType type) =>
            IngameData.Settings.shooterSkinsSettings.GetUiOutlineSprite(type);


        public Sprite GetPetSkinSprite(PetSkinType type) =>
            IngameData.Settings.pets.skinsSettings.GetSkinUiSprite(type);


        protected override RewardData[] SelectSequenceRewardData(RewardData[] allCurrentSequenceReward) =>
            allCurrentSequenceReward;


        // dirty way. here we also select data for sequence
        protected override RewardData[] GetSequenceData(int showIndex)
        {
            SegmentData[] sequenceSegmentData = GetSequenceSegmentsData(showIndex);
            RewardData[] result = new RewardData[sequenceSegmentData.Length];

            int missedElements = 0;

            for (int i = 0; i < sequenceSegmentData.Length; i++)
            {
                RewardData segmentReward = GenerateSegmentReward(sequenceSegmentData[i]);
                if (segmentReward != null)
                {
                    result[i] = segmentReward;
                }
                else
                {
                    missedElements++;
                }
            }

            if (missedElements > 0)
            {
                CustomDebug.Log($"Wrong logic detected. Data must be full. Trying to fill with common reward pack data");

                for (int i = 0; i < segmentData.Length; i++)
                {
                    if (result[i] == null)
                    {
                        RewardData generatedCommonData = GenerateSegmentReward(segmentData[i]);
                        result[i] = generatedCommonData;
                    }
                }
            }

            return result;
        }


        public override RewardData[] GetCommonShowRewardPack()
        {
            SegmentData[] sequenceSegmentData = GetSequenceSegmentsData();
            
            List<ShooterSkinReward> ignoreShooterSkinRewards = new List<ShooterSkinReward>();
            List<WeaponSkinReward> ignoreWeaponSkinReward = new List<WeaponSkinReward>();
            
            foreach (var item in sequenceSegmentData)
            {
                ignoreShooterSkinRewards.AddRange(item.shooterSkinRewards);
                ignoreWeaponSkinReward.AddRange(item.weaponSkinRewards);
            }
            
            List<RewardData> result = new List<RewardData>();
            
            foreach (var data in segmentData)
            {
                RewardData segmentReward = GenerateSegmentReward(data, 
                    ignoreShooterSkinRewards, 
                    ignoreWeaponSkinReward);
                
                result.Add(segmentReward);
            }

            return result.ToArray();
        }


        private RewardData GenerateSegmentReward(SegmentData sourceSegmentData, List<ShooterSkinReward> ignoreShooterSkinRewards = null, List<WeaponSkinReward> ignoreWeaponSkinReward = null)
        {
            RewardData[] allCommonRewardForSegment = sourceSegmentData.AllCommonRewards;
            
            allCommonRewardForSegment = SelectAvailableRewardData(allCommonRewardForSegment);
            
            if (ignoreShooterSkinRewards != null)
            {
                allCommonRewardForSegment =
                    RewardDataUtility.RemoveIgnoredCharacterSkinRewards(allCommonRewardForSegment,
                        ignoreShooterSkinRewards);
            }

            if (ignoreWeaponSkinReward != null)
            {
                allCommonRewardForSegment =
                    RewardDataUtility.RemoveIgnoredWeaponSkinRewards(allCommonRewardForSegment,
                        ignoreWeaponSkinReward);
            }

            bool isAnyCommonRewardAvailable = allCommonRewardForSegment.Any();

            RewardData[] rewardsList = 
                isAnyCommonRewardAvailable ? 
                    allCommonRewardForSegment : 
                    sourceSegmentData.premiumCurrencyRewards;
            
            RewardData result = RewardDataUtility.GetRandomReward(rewardsList);

            return result;
        }


        public Sprite FindCurrencyRewardSprite(CurrencyType type)
        {
            CurrencySpritesSettings settings = Array.Find(currencySpritesSettings, e => e.type == type);
            
            return settings?.sprite;
        }

        #endregion



        #region Editor methods

        private void OnValidate()
        {
            int i = 0;

            foreach (var segment in segmentData)
            {
                List<RewardData> allReward = new List<RewardData>();
                allReward.AddRange(segment.shooterSkinRewards);
                allReward.AddRange(segment.weaponSkinRewards);
                allReward.AddRange(segment.currencyRewards);
                allReward.AddRange(segment.premiumCurrencyRewards);

                foreach (var reward in allReward)
                {
                    reward.receiveType = i == 0 ? RewardDataReceiveType.Default : RewardDataReceiveType.Video;
                }

                i++;
            }
        }

        #endregion
    }
}
