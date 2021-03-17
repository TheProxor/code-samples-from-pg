using UnityEngine;


namespace Drawmasters.Interfaces
{
    public interface ICreate<T>
    {
        T CreatedObject { get; }

        void Create();
    }
}
