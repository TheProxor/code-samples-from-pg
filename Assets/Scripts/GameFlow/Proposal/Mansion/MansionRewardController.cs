using System;
using System.Collections.Generic;
using Drawmasters.Utils;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Proposal
{
    public class MansionRewardController
    {
        #region Fields

        private readonly List<RealtimeTimer> refreshTimers = new List<RealtimeTimer>();

        #endregion



        #region Properties

        public bool WasFreeKeyReceived =>
            CustomPlayerPrefs.HasKey(PrefsKeys.Proposal.FreeMansionKey);

        public DateTime NearNextRefreshDate
        {
            get
            {
                DateTime result = DateTime.MaxValue;
                //TODO hotfix
                bool isTimeChanged = false;

                for (int i = 0; i < PlayerMansionData.MansionRoomsCount; i++)
                {
                    if (IsTimerActive(i))
                    {
                        DateTime nextDate = GetNextRefreshDate(i);

                        if (nextDate < result)
                        {
                            result = nextDate;
                            isTimeChanged = true;
                        }
                    }
                }

                return isTimeChanged ? result : DateTime.MinValue;
            }
        }

        #endregion



        #region Class lifecycle

        public MansionRewardController(ITimeValidator _timeValidator)
        {
            for (int i = 0; i < PlayerMansionData.MansionRoomsCount; i++)
            {
                RealtimeTimer timer = new RealtimeTimer(string.Concat(PrefsKeys.Proposal.MansionRewardRoomNewTimerPrefix, i), _timeValidator);

                refreshTimers.Add(timer);
            }

        }

        #endregion



        #region Methods

        public void MarkFreeKeyReceived() =>
            CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.FreeMansionKey, true);


        public RealtimeTimer GetRefreshTimer(int roomIndex) =>
            refreshTimers.SafeGet(roomIndex);


        public bool IsTimerActive(int roomIndex)
        {
            RealtimeTimer timer = GetRefreshTimer(roomIndex);

            if (timer == null)
            {
                CustomDebug.Log("Timer is null. Room index : " + roomIndex);
            }

            return timer == null ? default : timer.IsTimerActive;
        }

        public bool IsOldUser(int roomIndex)
        {
            bool result;

            RealtimeTimer timer = new RealtimeTimer(string.Concat(PrefsKeys.Proposal.MansionRewardRoomTimerPrefix, roomIndex), GameServices.Instance.TimeValidator);

            result = timer.IsTimerActive;

            timer.Stop();
            timer.Deinitialize();

            return result;
        }

        public CurrencyReward RewardForRoom(int index)
        {
            CurrencyReward result = null;

            var mansionRoomsRewards = GameServices.Instance.AbTestService.CommonData.mansionRoomsRewards;
            if (mansionRoomsRewards.TryGetValue(index, out CurrencyRewardData currentRoomReward))
            {
                result = new CurrencyReward()
                {
                    value = currentRoomReward.value,
                    currencyType = currentRoomReward.currencyType
                };
            }
            else
            {
                CustomDebug.Log("Wrong reward access");
            }

            return result;
        }

        public void MarkRewardApplied(int roomIndex)
        {
            RealtimeTimer timer = GetRefreshTimer(roomIndex);

            if (timer != null)
            {
                timer.Stop();
                timer.Deinitialize();

                timer.Initialize();
                timer.Start(GetCooldownForPassiveIncome(roomIndex));
            }
        }

        private DateTime GetNextRefreshDate(int roomIndex)
        {
            RealtimeTimer timer = GetRefreshTimer(roomIndex);
            TimeSpan timeSpan = timer == null ? TimeSpan.Zero : timer.TimeLeft;
            return DateTime.Now.Add(timeSpan);
        }


        private float GetCooldownForPassiveIncome(int roomIndex) =>
            IngameData.Settings.mansionRewardPackSettings.GetCurrencyPassiveIncomeCooldown(roomIndex);
    }

    #endregion
}
