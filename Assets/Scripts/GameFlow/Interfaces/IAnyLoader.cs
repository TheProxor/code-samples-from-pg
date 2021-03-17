namespace Drawmasters
{
    public interface IAnyLoader<T>
    {
        bool IsLoaded { get; }

        T LoadedObject { get; }

        void Load();
    }
}

