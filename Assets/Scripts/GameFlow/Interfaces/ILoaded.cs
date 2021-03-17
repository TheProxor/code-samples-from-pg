namespace Drawmasters.Levels
{
    public interface ILoaded<T>
    {
        bool IsLoaded { get; }

        void Load(T data);

        void Unload();
    }
}
