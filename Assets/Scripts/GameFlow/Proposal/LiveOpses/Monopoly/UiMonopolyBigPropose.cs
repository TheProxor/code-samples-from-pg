using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal;
using TMPro;


namespace Drawmasters.Ui
{
    public class UiMonopolyBigPropose : UiMonopolyBasePropose
    {
        #region Fields

        private const float ScaleIconSkinsMultiplier = 0.76f;

        [SerializeField] private Image rewardImage = default;
        [SerializeField] private TMP_Text rewardText = default;
        
        #endregion


        
        #region Methods
        
        protected override void OnShouldRefreshVisual()
        {
            base.OnShouldRefreshVisual();

            if (controller.IsActive)
            {
                RewardData[] rewards = controller.GeneratedLiveOpsReward;
                if (rewards.Length == 0)
                {
                    return;
                }

                RewardData reward = rewards.LastObject();

                if (reward == null)
                {
                    return;
                }

                rewardText.text = string.Empty;

                switch (reward)
                {
                    case CurrencyReward currency:
                        rewardText.text = currency.UiRewardText;
                        rewardImage.sprite = IngameData.Settings.monopoly.visualSettings.FindLapsCurrencyActiveIcon(currency.currencyType);
                        break;

                    case ShooterSkinReward shooterSkinReward:
                        rewardImage.sprite = IngameData.Settings.monopoly.visualSettings.FindLapsShooterSkinActiveIcon(shooterSkinReward.skinType);
                        break;

                    case WeaponSkinReward weaponSkinReward:
                        rewardImage.sprite = IngameData.Settings.monopoly.visualSettings.FindLapsWeaponSkinActiveIcon(weaponSkinReward.skinType);
                        break;

                    default:
                        CustomDebug.Log($"Not implemented logic for {reward.Type}");
                        break;
                }


                bool isCurrencyReward = reward is CurrencyReward;
                rewardImage.transform.localScale = isCurrencyReward ? Vector3.one : Vector3.one * ScaleIconSkinsMultiplier;

                rewardImage.SetNativeSize();
            }
        }

        #endregion
    }
}
