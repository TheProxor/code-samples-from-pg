using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public abstract class SequenceForcemeterRewardPackSettings : SequenceRewardPackSettings
    {
        #region Nested types

        [Serializable]
        public class SequenceSegmentsData
        {
            public ForceMeterRewardPackSettings.SegmentData[] rewards = default;
        }

        #endregion



        #region Fields

        [Header("Sequence reward data")]
        [SerializeField] private SequenceSegmentsData[] sequenceData = default;

        #endregion



        #region Protected methods

        protected ForceMeterRewardPackSettings.SegmentData[] GetSequenceSegmentsData(int i)
        {
            if (i >= sequenceData.Length)
            {
                CustomDebug.Log($"Wrong logic detected. Trying get data with index {i} while total count is {sequenceData.Length}");
                
                return Array.Empty<ForceMeterRewardPackSettings.SegmentData>();
            }

            return sequenceData[i].rewards;
        }
        
        protected ForceMeterRewardPackSettings.SegmentData[] GetSequenceSegmentsData()
        {
            List<ForceMeterRewardPackSettings.SegmentData> list = new List<ForceMeterRewardPackSettings.SegmentData>();
            foreach (var item in sequenceData)
            {
                list.AddRange(item.rewards);                
            }
            
            return list.ToArray();
        }
         

        protected override bool CanProposeSequenceData(int i) =>
            i < sequenceData.Length && i >= 0;

        #endregion
    }
}
