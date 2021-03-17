using System;


namespace Drawmasters.Proposal
{
    public interface ILiveOpsFinishSoonNotification
    {
        bool AllowRegisterFinishSoonNotification { get; }

        DateTime FireDateTimeFinishSoonNotification { get; }
    }
}