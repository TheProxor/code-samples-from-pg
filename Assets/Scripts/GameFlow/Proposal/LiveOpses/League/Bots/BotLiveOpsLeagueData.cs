using System;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Drawmasters
{
    [Serializable]
    public class BotLiveOpsLeagueData : LeaderBoardItem
    {
        #region Filds

        public BotType type;
        public long lastWorkDay;
        public float receivedScoreTimePerLastDay;
        public float offlineSkullCount;
        public LeagueProposeController controller;

        #endregion

        

        #region Properties

        public override LeaderBordItemType ItemType => LeaderBordItemType.Bot;

        public float ReceivedScorePerTime { get; set; }

        public int BlokSkullCount { get; set; }

        public int UnblokSkullCount { get; set; }

        public int OfflineSkullsMax { get; set; }

        public int OfflineSkullsMin { get; set; }

        public ITimeValidator TimeValidator { get; set; }

        public override float SkullsCount
        {
            get => base.SkullsCount; 
            set
            {
                //HACK: for old users
                if (offlineSkullCount < base.SkullsCount)
                {
                    offlineSkullCount = base.SkullsCount;
                }

                offlineSkullCount = value;

                if (controller.IsInternetAvailable)
                {
                    base.SkullsCount = offlineSkullCount;
                }
            }
        }

        #endregion

        

        #region Class lifecycle

        public BotLiveOpsLeagueData(string _id, string _nickName, ShooterSkinType _skinType, BotType _type,
            LeagueProposeController _controller, ITimeValidator _timeValidator) : base(_id, _nickName, _skinType)
        {
            type = _type;
            TimeValidator = _timeValidator;
            lastWorkDay = TimeValidator.ValidNow.ToBinary();
            shooterSkinType = _skinType;
            isBloked = false;
            BlokSkullCount = int.MaxValue;

            receivedScoreTimePerLastDay = 0;

            controller = _controller;
        }

        #endregion


        
        #region Methods

        public void UpdateSkullsOldMechanic(float skullsMultiplier, float fulltime = 0)
        {
            float skulls = CalculateSkullsOldMethod(fulltime) * skullsMultiplier; 
            SkullsCount += skulls;
        }


        public void UpdateOnlineSkulls(float skullsMultiplier, float playerSkullCount)
        {
            if (isBloked)
            {
                if (Mathf.Abs(playerSkullCount - SkullsCount) <= UnblokSkullCount)
                {
                    isBloked = false;
                }
                else
                {
                    lastWorkDay = TimeValidator.ValidNow.ToBinary();
                    return;
                }
            }

            float skulls = CalculateSkulls() * skullsMultiplier;
            if (playerSkullCount > 0 && skulls > playerSkullCount * 2.0f)
            {
                skulls = playerSkullCount * 2.0f;
            }

            SkullsCount += skulls;

            isBloked = (SkullsCount - playerSkullCount) > BlokSkullCount;
        }


        public void UpdateOfflineSkulls(float skullsMultiplier, float playerSkullCount)
        {
            float skulls = CalculateSkulls() * skullsMultiplier;

            if (playerSkullCount > 0 && skulls > playerSkullCount * 2.0f)
            {
                skulls = playerSkullCount * 2.0f;
            }

            if (skulls > OfflineSkullsMax)
            {
                skulls = Random.Range(OfflineSkullsMin, OfflineSkullsMax);
            }

            SkullsCount += skulls;

            isBloked = (SkullsCount - playerSkullCount) > BlokSkullCount;
        }


        private float CalculateSkulls()
        {
            TimeSpan time = TimeValidator.ValidNow - DateTime.FromBinary(lastWorkDay);

            lastWorkDay = TimeValidator.ValidNow.ToBinary();

            float timeFactor = (float) time.TotalSeconds / IngameData.Settings.league.botSettings.сycleTimeSec;
            float typeFactor = IngameData.Settings.league.botSettings.GetDifficultyFactor(type);


            float skulls = (float) RandomExtentions.RandomGauss(ReceivedScorePerTime * typeFactor * timeFactor,
                IngameData.Settings.league.botSettings.randomSigma);
            skulls = Mathf.Round(skulls);
            return skulls < 0 ? 0 : skulls;
        }


        private float CalculateSkullsOldMethod(float time)
        {
            lastWorkDay = TimeValidator.ValidNow.ToBinary();
            receivedScoreTimePerLastDay += time;

            float timeFactor = time / IngameData.Settings.league.botSettings.сycleTimeSec;
            float typeFactor = IngameData.Settings.league.botSettings.GetDifficultyFactor(type);

            int skulls = Convert.ToInt32(RandomExtentions.RandomGauss(ReceivedScorePerTime * typeFactor * timeFactor,
                IngameData.Settings.league.botSettings.randomSigma));

            return skulls < 0 ? 0 : skulls;
        }

        #endregion
    }
}