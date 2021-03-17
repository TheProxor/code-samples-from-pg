using UnityEngine;

using UScene = UnityEngine.SceneManagement.Scene;

namespace Core
{
    [System.Serializable]
    public class LinkScene
    {
        public string Path => path;

        [SerializeField]
        private string path = null;
    }
}
