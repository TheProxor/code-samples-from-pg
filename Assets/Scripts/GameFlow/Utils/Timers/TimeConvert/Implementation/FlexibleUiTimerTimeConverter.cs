using System;
using Drawmasters.Utils.UiTimeProvider.Interfaces;

namespace Drawmasters.Utils.UiTimeProvider.Implementation
{
    public class FlexibleUiTimerTimeConverter : ITimeUiTextConverter
    {
        public string Convert(RealtimeTimer timer)
        {
            string result;
            
            TimeSpan span = timer.TimeLeft;

            if (span.Days > 0)
            {
                result = $"{span.Days}d:{span.Hours}h";
            }
            else if (span.Hours > 0)
            {
                result = $"{span.Hours}h:{span.Minutes}m";
            }
            else if (span.Minutes > 0)
            {
                result = $"{span.Minutes}m:{span.Seconds}s";
            }
            else
            {
                result = $"{span.Seconds}s";
            }

            return result;
        }
    }
}