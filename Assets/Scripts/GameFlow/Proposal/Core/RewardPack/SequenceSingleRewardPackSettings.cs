using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public abstract partial class SequenceSingleRewardPackSettings : SequenceRewardPackSettings
    {
        #region Fields

        [Header("Sequence reward data")]
        [SerializeField] protected SequenceData[] sequenceData = default;

        #endregion



        #region Protected methods

        protected override bool CanProposeSequenceData(int i) =>
            i < sequenceData.Length && i >= 0;


        protected override RewardData[] GetSequenceData(int i)
        {
            if (sequenceData.Length <= i || i < 0)
            {
                CustomDebug.Log($"Wrong logic detected in {this}. Trying to get {i} number for rewards data with length {sequenceData.Length}");
                
                return Array.Empty<RewardData>();
            }

            return sequenceData[i].RewardData;
        }
        
        protected RewardData[] GetSequenceData()
        {
            List<RewardData> list = new List<RewardData>();
            
            foreach (var item in sequenceData)
            {
                list.AddRange(item.RewardData);                
            }
            
            return list.ToArray();
        }

        #endregion

    }
}