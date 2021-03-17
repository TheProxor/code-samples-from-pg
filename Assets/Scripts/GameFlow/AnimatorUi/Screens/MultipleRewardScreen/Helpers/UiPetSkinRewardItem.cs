using Drawmasters.Proposal;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class UiPetSkinRewardItem : UiRewardItem
    {
        #region Fields

        [SerializeField] private SkeletonGraphic petAnimation = default;

        #endregion
        
        
        
        #region Overrided methods

        public override void InitializeUiRewardItem(RewardData _rewardData, int sortingOrder)
        {
            base.InitializeUiRewardItem(_rewardData, sortingOrder);
            
            ApplyVisual(_rewardData as PetSkinReward);
        }

        #endregion



        #region Private methods

        private void ApplyVisual(PetSkinReward reward)
        {
            bool isCorrectReward = reward != null && reward.skinType != PetSkinType.None;
            CommonUtility.SetObjectActive(petAnimation.gameObject, isCorrectReward);

            petAnimation.skeletonDataAsset = IngameData.Settings.pets.uiSettings.FindMainMenuSkeletonData(reward.skinType);

            petAnimation.Initialize(true);
        }

        #endregion
    }
}