using Drawmasters.Proposal;


namespace Drawmasters.Ui
{
    public interface IUiRewardReceiveBehaviour : IUiBehaviour
    {
        void SetupFxKey(string fxKey);
        
        void PlayFx();
        
        void StopFx();

        void SetupReward(params RewardData[] rewardData);
    }
}
