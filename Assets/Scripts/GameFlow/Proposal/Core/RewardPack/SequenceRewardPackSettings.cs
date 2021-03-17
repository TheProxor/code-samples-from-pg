namespace Drawmasters.Proposal
{
    public abstract class SequenceRewardPackSettings : RewardPackSettings
    {
        #region Properties

        protected abstract bool CanProposeSequenceData(int i);

        protected abstract RewardData[] GetSequenceData(int i);

        #endregion



        #region Public methods

        public abstract RewardData[] GetCommonShowRewardPack();

        public RewardData[] GetRewardPack(int showIndex)
        {
            RewardData[] result;

            if (CanProposeSequenceData(showIndex))
            {
                RewardData[] allCurrentSequenceReward = GetSequenceData(showIndex);
                result = SelectSequenceRewardData(allCurrentSequenceReward);
            }
            else
            {
                result = GetCommonShowRewardPack();
            }

            return result;
        }
        
        #endregion


        #region Protected

        protected abstract RewardData[] SelectSequenceRewardData(RewardData[] allCurrentSequenceReward);

        #endregion
    }
}
