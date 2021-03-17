using UnityEngine;

namespace Drawmasters 
{
    public class ResourceAsset<T> where T : Object
    {
        private readonly string path;
        private T asset;

        public ResourceAsset(string path)
        {
            this.path = path;
        }

        public T Value
        {
            get
            {
                asset = asset ?? Resources.Load<T>(path);
                return asset as T;
            }
        }
    }
}
