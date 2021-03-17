using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal;
using TMPro;
using Drawmasters.Effects;


namespace Drawmasters.Ui
{
    public abstract class UiLeagueLeaderBoardRewardRootBase : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image rewardImage = default;
        [SerializeField] private Image currecnyRewardImage = default;
        [SerializeField] private TMP_Text rewardCountText = default;
        [SerializeField] private IdleEffect idleEffect = default;

        protected LeagueProposeController controller;
        protected LeaderBordItemType currentOwnerType;
        protected RewardData rewardData;

        #endregion



        #region Properties

        public RewardData RewardData =>
            rewardData;

        protected virtual string IdleFxkey =>
            string.Empty;

        #endregion



        #region Methods

        public virtual void Deintiailize()
        {
            rewardData = null;
            idleEffect.StopEffect();
        }


        public void SetupController(LeagueProposeController _controller) =>
            controller = _controller;


        public void SetupOwnerType(LeaderBordItemType _currentOwnerType) =>
            currentOwnerType = _currentOwnerType;


        public void SetupRewardData(RewardData _rewardData)
        {
            rewardData = _rewardData;

            if (rewardData == null)
            {
                CustomDebug.Log($"Can't setup reward for <color=red>{this}</color>. Reward data is null");
            }
        }


        public void RefreshVisual()
        {
            if (rewardData != null)
            {
                OnRefreshVisual();
            }
        }


        protected virtual void OnRefreshVisual()
        {
            (Sprite sprite, float scaleMultiplier) = GetRewardSpriteAndScaleMultiplier();
            Sprite rewardSprite = sprite;
            Image imageToSet = rewardData.Type == RewardType.Currency ? currecnyRewardImage : rewardImage;

            imageToSet.sprite = rewardSprite;
            imageToSet.transform.localScale = Vector3.one * scaleMultiplier;
            imageToSet.SetNativeSize();

            CommonUtility.SetObjectActive(currecnyRewardImage.gameObject, rewardData.Type == RewardType.Currency);
            CommonUtility.SetObjectActive(rewardImage.gameObject, rewardData.Type != RewardType.Currency);

            string rewardCountTextValue = rewardData is CurrencyReward currencyReward ?
                currencyReward.UiRewardText : string.Empty;

            rewardCountText.text = rewardCountTextValue;

            if (!idleEffect.IsKeyEquals(IdleFxkey) ||
                !idleEffect.IsCreated)
            {
                idleEffect.StopEffect();
                idleEffect.SetFxKey(IdleFxkey);
                idleEffect.CreateAndPlayEffect();
            }
        }


        protected abstract (Sprite, float) GetRewardSpriteAndScaleMultiplier();

        #endregion
    }
}
