using System;
using Modules.General.Abstraction;
using Drawmasters.Proposal;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public abstract class UiRewardElement : MonoBehaviour, IDeinitializable
    {
        #region Nested types

        public enum State
        {
            None = 0,
            Free = 1,
            ForVideo = 2,
            Received = 3,
            ForCurrency = 4
        }

        public enum RecievePlacement
        {
            None = 0,
            Free = 1,
            ForVideo = 2,
            ForCurrency = 3
        }


        [Serializable]
        private class StateVisual
        {
            public State state = default;
            public GameObject root = default;
        }


        [Serializable]
        private class ReceivedVisual
        {
            public RewardType rewardType = default;
            public GameObject root = default;
        }

        #endregion



        #region Fields

        public event Action<UiRewardElement, RecievePlacement> OnShouldReceive;

        protected const string CurrencyFormat = "+{0}";

        [Header("Common settings")]
        [SerializeField] private Button receiveButton = default;
        [SerializeField] private RewardedVideoButton videoButton = default;

        [Header("Visual settings")]
        [SerializeField] private ReceivedVisual[] receivedVisuals = default;
        [SerializeField] private StateVisual[] stateVisuals = default;

        private State currentState;

        #endregion



        #region Properties

        public bool IsReceived => currentState == State.Received;

        public bool IsForCurrency => currentState == State.ForCurrency;

        public bool IsForVideo => currentState == State.ForVideo;

        public RewardData RewardData { get; private set; }

        public abstract string AdsPlacement { get; }

        #endregion



        #region Methods

        public void InitializeVideoButton()
        {
            videoButton.Initialize(AdsPlacement);
            videoButton.OnVideoShowEnded += VideoButton_OnPressed;
        }


        public virtual void Initialize(RewardData rewardData)
        {
            RewardData = rewardData;
        }


        public virtual void Deinitialize()
        {
            videoButton.Deinitialize();
            videoButton.OnVideoShowEnded -= VideoButton_OnPressed;
        }


        public void SetState(State state)
        {
            currentState = state;

            RefreshVisual();
        }


        public virtual void InitializeButtons()
        {
            if (receiveButton != null)
            {
                receiveButton.onClick.AddListener(OnFreeReceived);
            }

            videoButton.InitializeButtons();
        }


        public virtual void DeinitializeButtons()
        {
            if (receiveButton != null)
            {
                receiveButton.onClick.RemoveListener(OnFreeReceived);
            }

            videoButton.DeinitializeButtons();
        }


        private void RefreshVisual()
        {
            foreach (var data in stateVisuals)
            {
                CommonUtility.SetObjectActive(data.root, data.state == currentState);
            }

            if (RewardData != null)
            {
                foreach (var data in receivedVisuals)
                {
                    CommonUtility.SetObjectActive(data.root, data.rewardType == RewardData.Type);
                }
            }
        }


        protected virtual void ReceiveReward(RecievePlacement placement) =>
            OnShouldReceive?.Invoke(this, placement);


        protected virtual void OnFreeReceived() =>
            ReceiveReward(RecievePlacement.Free);


        protected virtual void OnReceivedFromVideo() =>
            ReceiveReward(RecievePlacement.ForVideo);

        #endregion



        #region Events handlers

        private void VideoButton_OnPressed(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                OnReceivedFromVideo();
            }
        }

        #endregion
    }
}
