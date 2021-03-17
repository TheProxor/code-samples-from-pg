using System;
using Drawmasters.Effects;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using Drawmasters.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Modules.Sound;

namespace Drawmasters.Ui.Mansion
{
    public class UiMansionRewardPropose : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        private class RewardButtonInfo
        {
            public TMP_Text rewardCountText = default;

            public GameObject root = default;

            public Image progressBarImage = default;

            public void Enable()
            {
                CommonUtility.SetObjectActive(root, true);
            }

            public void Disable()
            {
                CommonUtility.SetObjectActive(root, false);

                progressBarImage.fillAmount = 0;
                rewardCountText.text = string.Empty;
            }
        }

        #endregion



        #region Fields

        public event Action<int> OnClaimStart;
        public event Action<int> OnClaimEnd;

        public event Action<RewardData> OnShouldReceiveReward;

        private const string ReceiveTextFormat = "+{0}";

        [Header("General")]
        [SerializeField] private GameObject root = default;

        [SerializeField] private Button claimButton = default;

        [SerializeField] private RectTransform currencyRewardRoot = default;

        [SerializeField] private TMP_Text perHourRewardCountText = default;

        [SerializeField] private GameObject perHourRewardCountTextRoot = default;

        [SerializeField] private RewardButtonInfo availableRewardButtonInfo = default;
        [SerializeField] private RewardButtonInfo unavailableRewardButtonInfo = default;

        [Header("Animation")]
        [SerializeField] private FactorAnimation progressBarAnimation = default;


        private LoopedInvokeTimer loopedInvokeTimer;

        private CurrencyReward currencyReward;
        private CurrencyReward currencyRewardMax;

        private RewardButtonInfo currentRewardButtonInfo;

        private int roomIndex;

        private bool canClaim;
        private bool wasClaimingApplied;

        private float rewardProgress;
        private float valueToDisplay;

        private int rewardCountPerHour;

        #endregion



        #region Properties

        private RewardButtonInfo CurrentRewardButtonInfo
        {
            get => currentRewardButtonInfo;
            set
            {
                currentRewardButtonInfo?.Disable();

                currentRewardButtonInfo = value;

                currentRewardButtonInfo.Enable();
            }
        }

        #endregion



        #region Methods

        public void SetEnabled(bool enabled) =>
            CommonUtility.SetObjectActive(root, enabled);


        public void SetupRoom(int _roomIndex)
        {
            roomIndex = _roomIndex;

            Refresh();
        }


        public void Initialize()
        {
            currencyReward = GameServices.Instance.ProposalService.MansionRewardController.RewardForRoom(roomIndex);
            currencyReward.value = 0;

            //restore old users progress
            for (int i = 0; i < PlayerMansionData.MansionRoomsCount; i++)
            {
                if (GameServices.Instance.ProposalService.MansionRewardController.IsOldUser(i))
                {
                    GameServices.Instance.ProposalService.MansionRewardController.MarkRewardApplied(i);
                }
            }

            loopedInvokeTimer = loopedInvokeTimer ?? new LoopedInvokeTimer(RefreshReward);
            loopedInvokeTimer.Start();

            RefreshReward();

            wasClaimingApplied = true;

            availableRewardButtonInfo.Disable();
            unavailableRewardButtonInfo.Disable();
        }


        public void Deinitialize()
        {
            loopedInvokeTimer.Stop();

            DOTween.Kill(this);
        }


        public void InitializeButtons() => claimButton.onClick.AddListener(ClaimButton_OnClick);


        public void DeinitializeButtons() => claimButton.onClick.RemoveListener(ClaimButton_OnClick);


        public bool TryFindReward(RewardData rewardData, out Vector3 pos)
        {
            pos = currencyRewardRoot.position;

            return true;
        }


        private void Refresh() => RefreshReward();


        private void RefreshReward()
        {
            currencyRewardMax = GameServices.Instance.ProposalService.MansionRewardController.RewardForRoom(roomIndex);

            CalculateRewardToClaim();
            UpdateButtonAvailable();
            UpdateRewardCountText();
            UpdateProgressBar();
            UpdatePerHourValue();

            void UpdateButtonAvailable()
            {
                CurrentRewardButtonInfo = canClaim ? availableRewardButtonInfo : unavailableRewardButtonInfo;

                claimButton.interactable = canClaim;
            }

    
            void CalculateRewardToClaim()
            {
                RealtimeTimer timer = GameServices.Instance.ProposalService.MansionRewardController.GetRefreshTimer(roomIndex);

                TimeSpan totalTime = timer.FinishTime - timer.StartTime;
                double totalSeconds = totalTime.TotalSeconds;
                double currentTimeSeconds = totalSeconds - timer.TimeLeft.TotalSeconds;

                rewardCountPerHour = (int)(currencyRewardMax.value / (int)totalTime.TotalHours);

                float percent = (float)(currentTimeSeconds / totalSeconds);

                currencyReward.value = Mathf.FloorToInt(currencyRewardMax.value * percent);

                if (wasClaimingApplied)
                {
                    rewardProgress = percent;
                }

                canClaim = currencyReward.value > 0;
            }


            void UpdatePerHourValue()
            {
                CommonUtility.SetObjectActive(perHourRewardCountTextRoot, rewardCountPerHour > 0);
                perHourRewardCountText.text = rewardCountPerHour.ToString();
            }
        }

        void UpdateProgressBar() => CurrentRewardButtonInfo.progressBarImage.fillAmount = rewardProgress;


        void UpdateRewardCountText()
        {
            valueToDisplay = Mathf.FloorToInt(currencyRewardMax.value * rewardProgress);

            string text = $"{valueToDisplay}/{currencyRewardMax.value}";

            CurrentRewardButtonInfo.rewardCountText.text = text;
        }

        #endregion



        #region Events handlers

        private void ClaimButton_OnClick()
        {
            if (!canClaim)
            {
                return;
            }

            wasClaimingApplied = false;

            OnClaimStart?.Invoke(roomIndex);
            OnShouldReceiveReward?.Invoke(currencyReward);

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIButtonNextClick, parent: claimButton.transform, transformMode: TransformMode.Local);
            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.CHALLENGE_PROGRESSBAR_VERTICAL);

            progressBarAnimation.beginValue = rewardProgress;
            progressBarAnimation.Play(PlayAnimation, this, OnClaimApplied);

            Refresh();

            void PlayAnimation(float value)
            {
                rewardProgress = value;

                UpdateProgressBar();
                UpdateRewardCountText();
            }

            void OnClaimApplied()
            {
                wasClaimingApplied = true;

                OnClaimEnd?.Invoke(roomIndex);
            }
        }

        #endregion
    }
}
