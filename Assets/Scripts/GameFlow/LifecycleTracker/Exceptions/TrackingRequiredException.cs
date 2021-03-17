using System;

namespace Drawmasters
{
    public class TrackingRequiredException : Exception
    {
        public TrackingRequiredException(string message = null) : base(message)
        {
            
        }
    }
}