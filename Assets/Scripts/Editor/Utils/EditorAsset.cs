using UnityEngine;
using UnityEditor;


namespace Drawmasters.Editor.Utils
{
    public class EditorAsset<T> where T : Object
    {
        readonly string path;
        T asset;

        public EditorAsset(string path)
        {
            this.path = path;
        }

        public T Value
        {
            get
            {
                asset = asset ?? AssetDatabase.LoadAssetAtPath<T>(path);
                return asset;
            }
        }
    }
}
