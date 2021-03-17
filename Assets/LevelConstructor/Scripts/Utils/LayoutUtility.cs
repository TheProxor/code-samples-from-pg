using System.IO;
using System.Reflection;
using Type = System.Type;


namespace Drawmasters.Editor.Utils
{
    public static class LayoutUtility
    {
        #region Fields

        private static MethodInfo miLoadWindowLayout;
        private static MethodInfo miSaveWindowLayout;
        private static MethodInfo miReloadWindowLayoutMenu;

        private static bool isAvailable;
        private static string layoutsPath;

        #endregion
        
        
        
        #region Properties

        /// <summary>
        /// Gets a value indicating whether all required Unity API
        /// functionality is available for usage.
        /// </summary>
        public static bool IsAvailable => isAvailable;


        /// <summary>
        /// Gets absolute path of layouts directory.
        /// Returns `null` when not available.
        /// </summary>
        public static string LayoutsPath => layoutsPath;

        #endregion



        #region Class lifecycle

        static LayoutUtility()
        {
            Type tyWindowLayout = Type.GetType("UnityEditor.WindowLayout,UnityEditor");
            Type tyEditorUtility = Type.GetType("UnityEditor.EditorUtility,UnityEditor");
            Type tyInternalEditorUtility = Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor");

            if (tyWindowLayout != null && tyEditorUtility != null && tyInternalEditorUtility != null)
            {
                MethodInfo miGetLayoutsPath = tyWindowLayout.GetMethod("GetLayoutsPath",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                miLoadWindowLayout = tyWindowLayout.GetMethod("LoadWindowLayout",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null,
                    new Type[] {typeof(string), typeof(bool)}, null);
                miSaveWindowLayout = tyWindowLayout.GetMethod("SaveWindowLayout",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null,
                    new Type[] {typeof(string)}, null);
                miReloadWindowLayoutMenu = tyInternalEditorUtility.GetMethod("ReloadWindowLayoutMenu",
                    BindingFlags.Public | BindingFlags.Static);

                if (miGetLayoutsPath == null || miLoadWindowLayout == null || miSaveWindowLayout == null ||
                    miReloadWindowLayoutMenu == null)
                    return;

                layoutsPath = (string) miGetLayoutsPath.Invoke(null, null);
                if (string.IsNullOrEmpty(layoutsPath))
                    return;

                isAvailable = true;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Save current window layout to asset file.
        /// </summary>
        /// <param name="assetPath"> Must be relative to project directory.</param>
        public static void SaveLayoutToAsset(string assetPath)
        {
            SaveLayout(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
        }

        
        /// <summary>
        /// Load window layout from asset file.
        /// </summary>
        /// <param name="assetPath"> Must be relative to project directory.</param>
        public static void LoadLayoutFromAsset(string assetPath)
        {
            if (miLoadWindowLayout != null)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                miLoadWindowLayout.Invoke(null, new object[] {path, false});
            }
        }


        /// <summary>
        /// Save current window layout to file.
        /// </summary>
        /// <param name="path"> Must be absolute.</param>
        private static void SaveLayout(string path)
        {
            if (miSaveWindowLayout != null)
                miSaveWindowLayout.Invoke(null, new object[] {path});
        }

        #endregion
    }
}