#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace Drawmasters
{
    public class Content : SingletonMonoBehaviour<Content>, IInitializable
    {
        #region Fields

        #if UNITY_EDITOR
        // need load asset for access Storage in non playing mode
        private const string PrefabPath = "Assets/Prefabs/Core/Content.prefab";
        #endif

        [SerializeField] private ContentStorage contentStorage = default;

        private ContentManagement contentManagement;

        #endregion



        #region Properties

        public static ContentStorage Storage
        {
            get
            {
                ContentStorage result = default;

                if (Application.isPlaying)
                {
                    result = Instance.contentStorage;
                }
                else
                {
                    #if UNITY_EDITOR
                        var loadedContent = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Content)) as Content;

                        if (loadedContent == null)
                        {
                            CustomDebug.Log("Check path for Content! Asset was not loaded in non playing mode");
                        }
                        else
                        {
                            result = loadedContent.contentStorage;
                        }
                    #endif
                }

                return result;
            }
        }

        public static ContentManagement Management => Instance.contentManagement;

        #endregion



        #region Methods

        public void Initialize()
        {
            contentManagement = new ContentManagement();
        }

        #endregion
    }
}
