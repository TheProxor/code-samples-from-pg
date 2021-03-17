using System;


namespace Drawmasters
{
    [Flags]
    public enum AnalyticsType
    {
        None        = 0,
        AppsFlyer   = 1 << 2
    }
}
