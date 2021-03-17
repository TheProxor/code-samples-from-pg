namespace Drawmasters.Pool.Interfaces
{
    public interface IPoolCallback : IPoolableObject
    {
	    void OnPop();
	    void OnPush();
    }
}
