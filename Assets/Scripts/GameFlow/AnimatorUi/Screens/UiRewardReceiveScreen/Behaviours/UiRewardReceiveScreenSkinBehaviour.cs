using System;
using UnityEngine;
using UnityEngine.UI;
using Modules.Sound;
using Drawmasters.Effects;
using Drawmasters.Proposal;


namespace Drawmasters.Ui
{
    public class UiRewardReceiveScreenSkinBehaviour : IUiRewardReceiveBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public GameObject rootObject = default;

            public UiRewardReceiveScreenSkinHandler.Data skinData = default;

            public IdleEffect idleShineEffect = default;
            public Button applyButton = default;
        }

        #endregion




        #region Fields

        private readonly UiRewardReceiveScreenSkinHandler skinHandler;
        private readonly UiRewardReceiveScreen screen;
        private readonly Data data;

        private RewardData rewardData;

        #endregion



        #region Ctor

        public UiRewardReceiveScreenSkinBehaviour(Data _data, UiRewardReceiveScreen _screen)
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


        public void InitializeButtons()
        {
            data.applyButton.onClick.AddListener(ApplyButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            data.applyButton.onClick.RemoveListener(ApplyButton_OnClick);
        }


        public void Deinitialize()
        {
            Disable();
        }


        public void SetupFxKey(string fxKey) =>
            data.idleShineEffect.SetFxKey(fxKey);


        public void SetupReward(params RewardData[] _rewardData)
        {
            rewardData = _rewardData.FirstObject();

            skinHandler.SetupReward(rewardData);

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
            if (rewardData != null)
            {
                rewardData.Apply();
            }
            else
            {
                CustomDebug.LogWarning($"Attempt to receive null reward data in {this}");
            }

            screen.Hide();
        }

        #endregion
    }
}
