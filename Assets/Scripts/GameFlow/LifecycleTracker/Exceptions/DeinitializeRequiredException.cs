using System;

namespace Drawmasters
{
    public class DeinitializeRequiredException : Exception
    {
        public DeinitializeRequiredException(string message) : base(message)
        {
            
        }
    }
}