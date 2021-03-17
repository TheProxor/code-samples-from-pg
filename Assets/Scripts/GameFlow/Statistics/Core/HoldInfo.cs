using System;

namespace Drawmasters.Prefs
{
    public abstract class HoldInfo<T> where T : struct, IConvertible
    {
        public T key = default;
    }
}