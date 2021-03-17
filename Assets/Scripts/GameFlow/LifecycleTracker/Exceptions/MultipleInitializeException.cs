using System;

namespace Drawmasters
{
    public class MultipleInitializeException : Exception
    {
        public MultipleInitializeException(string message) : base(message)
        {
            
        }
    }
}