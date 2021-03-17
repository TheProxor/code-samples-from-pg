using Drawmasters.Proposal;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiWeaponSkinRewardItem : UiRewardItem
    {
        #region Fields

        [SerializeField] private Image weaponImage = default;

        #endregion
        
        
        
        #region Overrided methods
        
        public override void InitializeUiRewardItem(RewardData _rewardData, int sortingOrder)
        {
            base.InitializeUiRewardItem(_rewardData, sortingOrder);
            
            ApplyVisual(_rewardData as WeaponSkinReward);
        }
        
        #endregion



        #region Private methods

        private void ApplyVisual(WeaponSkinReward reward)
        {
            bool isCorrectReward = reward != null && reward.skinType != WeaponSkinType.None;
            CommonUtility.SetObjectActive(weaponImage.gameObject, isCorrectReward);

            if (!isCorrectReward)
            {
                return;
            }

            WeaponSkinSettings settings = IngameData.Settings.weaponSkinSettings;
            bool isResultSpriteExists = settings.TryGetSkinUiResultSprite(reward.skinType, out Sprite resultUiSprite);

            Sprite rewardSprite = isResultSpriteExists ? resultUiSprite : settings.GetSkinUiSprite(reward.skinType);
            weaponImage.sprite = rewardSprite;
            weaponImage.SetNativeSize();

            Vector3 imageScale = isResultSpriteExists ? Vector3.one : Vector3.one * 3.0f;
            weaponImage.rectTransform.localScale = imageScale;
        }

        #endregion
    }
}
