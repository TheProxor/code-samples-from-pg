namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IRateUsService
    {
        bool IsRated { get; }

        void RateApplication();

        bool IsAvailable { get; }
        bool IsRewardAvailable { get; }

        string UiRewardText { get; }

        void ApplyReward();

        string GetDescriptionText(bool isRated);
    }
}

