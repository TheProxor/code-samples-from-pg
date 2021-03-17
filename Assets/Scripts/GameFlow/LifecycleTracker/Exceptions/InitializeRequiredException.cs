using System;

namespace Drawmasters
{
    public class InitializeRequiredException : Exception
    {
        public InitializeRequiredException(string message) : base(message)
        {
            
        }
    }
}