using UnityEngine;


namespace Drawmasters.Pool.Interfaces
{
    public interface IPoolManager
    {
        ComponentPool GetComponentPool(MonoBehaviour component, bool autoCreate = true, int preInstantiateCount = 1);
    }
}

