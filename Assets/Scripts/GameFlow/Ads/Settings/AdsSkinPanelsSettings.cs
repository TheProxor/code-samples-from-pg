using Drawmasters.Utils;
using Drawmasters.ServiceUtil;
using UnityEngine;
using System;
using Drawmasters.Utils.UiTimeProvider.Implementation;
using Drawmasters.Utils.UiTimeProvider.Interfaces;


namespace Drawmasters.Proposal
{
    public abstract class AdsSkinPanelsSettings : ScriptableObject
    {
        #region Fields

        private RealtimeTimer cooldownTimer;

        protected WeaponType availableWeaponType;
        
        protected readonly ITimeUiTextConverter defaultTimeConverter = new FlexibleUiTimerTimeConverter();

        #endregion



        #region Properties

        public bool IsCoolDownExpired =>
            cooldownTimer.IsTimeOver;

        public string UiTimeLeft =>
            cooldownTimer.ConvertTime(defaultTimeConverter);
        
        public bool IsAnyCommonRewardAvailable =>
            CommonAvailableRewards.Length > 0;

        protected abstract float ButtonCooldown { get; }

        protected abstract string TimerPostfix { get; }
        
        protected abstract RewardData[] CommonAvailableRewards { get; }

        #endregion



        #region Methods

        public virtual void Initialize(Action onTimerRefreshCallback = null)
        {
            ITimeValidator timeValidator = GameServices.Instance.TimeValidator;
            cooldownTimer = new RealtimeTimer(TimerPostfix, timeValidator);
            cooldownTimer.Initialize();
            cooldownTimer.SetTimeOverCallback(onTimerRefreshCallback);

        }


        public RewardData GetRandomReward(int showIndex)
        {
            RewardData result;

            if (ShouldProposeSequenceReward(showIndex))
            {
                RewardData sequenceRewardData = GetSequenceReward(showIndex);

                result = IsRewardAvailable(sequenceRewardData) ? sequenceRewardData : RewardDataUtility.GetRandomReward(CommonAvailableRewards);
            }
            else
            {
                result = RewardDataUtility.GetRandomReward(CommonAvailableRewards);
            }

            return result;
        }


        public void SetAvailableWeaponType(WeaponType weaponType) =>
            availableWeaponType = weaponType;


        public virtual void MarkVideoWatched(Action onTimerRefreshCallback = null)
        {
            cooldownTimer.Start(ButtonCooldown);
            cooldownTimer.SetTimeOverCallback(onTimerRefreshCallback);
        }

        protected abstract bool ShouldProposeSequenceReward(int showIndex);

        protected abstract RewardData GetSequenceReward(int showIndex);

        protected abstract bool IsRewardAvailable(RewardData data);
        
        #endregion
    }
}
