namespace Drawmasters.Utils
{
    public enum TimeValidationStatus : byte
    {
        None            = 0,
        NotRequested    = 1,
        WaitingResponse = 2,
        GotResponse     = 3
    }
}
