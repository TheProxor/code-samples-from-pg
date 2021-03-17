namespace Drawmasters.Pool.Interfaces
{
    public interface IPool<T> : IInitializable
    {
        bool CanHandle(T prefab);

        void Push(T prefab);

        T Pop();
    }
}

    
