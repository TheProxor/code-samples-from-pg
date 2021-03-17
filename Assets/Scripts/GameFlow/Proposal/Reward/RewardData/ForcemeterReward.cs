using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class ForcemeterReward : RewardData
    {
        #region Fields

        private SequenceRewardPackSettings sequenceRewardPackSettings;
        private Action hidedCallback;

        #endregion



        #region Properties

        public override RewardType Type =>
            RewardType.Forcemeter;


        public override bool IsAvailableForRewardPack =>
            true;


        public override string RewardText =>
            "Forcemeter";

        #endregion



        #region Methods

        public void SetupRewardPackSettings(SequenceRewardPackSettings _settings) =>
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
            GameServices.Instance.ProposalsLoader.ForcemeterRewardDataShow(currentGameMode, sequenceRewardPackSettings, hidedCallback);
        }


        public override Sprite GetUiSprite() =>
            default;

        public Sprite GetForcemeterRewardSprite() =>
            default;

        #endregion
    }
}