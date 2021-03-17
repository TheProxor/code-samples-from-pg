using System.Collections.Generic;


namespace Drawmasters.Helpers.Interfaces
{
    interface IAssetLoader<T> where T : UnityEngine.Object
    {
        List<T> LoadAssets(string path);
    }
}
