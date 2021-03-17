using System;


namespace Drawmasters.Proposal
{
    public interface ILiveOpsNextStartNotification
    {
        bool AllowRegisterNextStartNotification { get; }

        DateTime FireDateTimeNextStartNotification { get; }
    }
}