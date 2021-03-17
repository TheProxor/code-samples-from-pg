using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class SpinRouletteReward : RewardData
    {
        #region Fields

        protected SpinRouletteSettings sequenceRewardPackSettings;
        protected Action hidedCallback;

        #endregion



        #region Properties

        public override RewardType Type =>
            RewardType.SpinRoulette;


        public override bool IsAvailableForRewardPack =>
            true;


        public override string RewardText =>
            "Spin Roulette Reward";

        #endregion



        #region Methods

        public void SetupRewardPackSettings(SpinRouletteSettings _settings) =>
            sequenceRewardPackSettings = _settings;
        
        public void SetupHidedCallback(Action _hidedCallback) =>
            hidedCallback = _hidedCallback;

        
        public override void Open()
        {
            base.Open();

            if (sequenceRewardPackSettings == null)
            {
                CustomDebug.LogError("No reward pack data set for forcemeter reward.");
                return;
            }

            GameMode currentGameMode = GameMode.Draw;
            GameServices.Instance.ProposalsLoader.SpinRouletteRewardDataShow(currentGameMode, sequenceRewardPackSettings, hidedCallback);
        }


        public override Sprite GetUiSprite() =>
            default;

        #endregion
    }
}
