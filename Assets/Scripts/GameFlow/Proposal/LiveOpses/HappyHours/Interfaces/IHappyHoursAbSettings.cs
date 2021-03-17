namespace Drawmasters.Proposal.Interfaces
{
    public interface IHappyHoursAbSettings
    {
        bool IsAvailable { get; }

        float DurationSeconds { get; }

        float StartSecondsBeforeLiveOpsFinish { get; }
    }
}
