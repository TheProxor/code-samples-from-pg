using UnityEngine;


namespace Drawmasters.Pool.Interfaces
{
    public interface IPoolHelper<TKey, TComponent> where TComponent : MonoBehaviour
    {
        // TODO autocreate bool
        TComponent PopObject(TKey key);

        void PushObject(TComponent component);
    }
}
