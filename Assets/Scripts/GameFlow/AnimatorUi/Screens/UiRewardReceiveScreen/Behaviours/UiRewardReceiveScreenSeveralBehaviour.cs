using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Modules.Sound;
using Drawmasters.Effects;
using Drawmasters.Helpers;
using Drawmasters.Proposal;


namespace Drawmasters.Ui
{
    public class UiRewardReceiveScreenSeveralBehaviour : IUiRewardReceiveBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public GameObject rootObject = default;

            public UiRewardReceiveScreenSkinHandler.Data skinData = default;
            public UiCurrencyData[] uiCurrencyData = default;

            public IdleEffect idleShineEffect = default;
            public Button applyButton = default;
        }

        #endregion




        #region Fields

        private readonly UiRewardReceiveScreenSkinHandler skinHandler;
        private readonly UiRewardReceiveScreen screen;
        private readonly Data data;

        private RewardData[] rewardData;

        #endregion



        #region Ctor

        public UiRewardReceiveScreenSeveralBehaviour(Data _data, UiRewardReceiveScreen _screen)
        {
            data = _data;
            screen = _screen;
            skinHandler = new UiRewardReceiveScreenSkinHandler(data.skinData);
        }

        #endregion



        #region IResultBehaviour

        public void Enable()
        {
            CommonUtility.SetObjectActive(data.rootObject, true);
        }


        public void Disable()
        {
            CommonUtility.SetObjectActive(data.rootObject, false);
        }


        public void Deinitialize()
        {
            Disable();
        }


        public void InitializeButtons()
        {
            data.applyButton.onClick.AddListener(ApplyButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            data.applyButton.onClick.RemoveListener(ApplyButton_OnClick);
        }


        public void SetupFxKey(string fxKey) =>
            data.idleShineEffect.SetFxKey(fxKey);


        public void SetupReward(params RewardData[] _rewardData)
        {
            rewardData = _rewardData;

            RewardData skinReward = rewardData.Where(e => RewardData.SkinsRewardTypes.Contains(t => t == e.Type)).FirstOrDefault();

            if (skinReward != null)
            {
                skinHandler.SetupReward(skinReward);
            }

            CurrencyReward[] currencyRewardData = rewardData.Where(e => e.Type == RewardType.Currency).Select(e => e as CurrencyReward).ToArray();

            foreach (var currencyData in data.uiCurrencyData)
            {
                CurrencyReward[] currentCurrencyReward = Array.FindAll(currencyRewardData, e => e.currencyType == currencyData.currencyType);

                bool isAnyCurrencyReceived = currentCurrencyReward.Any();
                CommonUtility.SetObjectActive(currencyData.root, isAnyCurrencyReceived);

                string totalCurrencyCount = currentCurrencyReward.Sum(e => e.value).ToShortFormat();
                currencyData.text.text = totalCurrencyCount;
            }

            PlayFx();
        }


        public void PlayFx()
        {
            data.idleShineEffect.CreateAndPlayEffect(20);
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SKIN_NEW_OPEN);
        }


        public void StopFx() =>
            data.idleShineEffect.StopEffect();

        #endregion



        #region Events handlers

        private void ApplyButton_OnClick()
        {
            foreach (var rd in rewardData)
            {
                rd.Apply();
            }

            screen.Hide();
        }

        #endregion
    }
}
