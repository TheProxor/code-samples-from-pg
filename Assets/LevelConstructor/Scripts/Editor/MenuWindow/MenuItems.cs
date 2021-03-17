using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drawmasters.Editor.Utils;
using Drawmasters.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = System.Object;


namespace Drawmasters
{
    public static class MenuItems
    {
        #region Fields

        private static readonly string RootLayoutSettingsPath = Path.Combine(Application.dataPath, "LevelConstructor", "Layouts");

        private static readonly string EditorLayoutSettingsPath = Path.Combine(RootLayoutSettingsPath, "Editor.wlt");
        private static readonly string EditorScenePath = Path.Combine(Application.dataPath, "LevelConstructor" , "Scenes", "Main.unity");
        
        private static readonly string GameLayoutSettingsPath = Path.Combine(RootLayoutSettingsPath, "Game.wlt");
        private static readonly string GameScenePath = Path.Combine(Application.dataPath, "Scenes", "MainScene.unity");

        private static Object[] contentImportedAssets;

        #endregion



        #region Methods

        [MenuItem("Drawmasters/Content/IMenuItemRefreshable: Refresh Assets")]
        private static void RefreshAssets()
        {
            Object[] assets = ResourcesUtility.LoadAllObjects<IMenuItemRefreshable>();

            foreach (var loadedAsset in assets)
            {
                (loadedAsset as IMenuItemRefreshable)?.RefreshFromMenuItem();
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Drawmasters/Content/IContentImport: Load IContentImport Assets (Long operation)")]
        private static void RefreshIContentImportAssets() =>
            contentImportedAssets = ResourcesUtility.LoadAllObjects<IContentImport>();


        [MenuItem("Drawmasters/Content/IContentImport: Reimport From Google Sheet")]
        private static void ReimportAllContent()
        {
            var contentSettings = ResourcesUtility.LoadAssetsByType(typeof(CommonContentSettings)).FirstOrDefault() as CommonContentSettings;

            if (contentSettings != null)
            {
                contentSettings.RefreshGoogleToken(() =>
                {
                    if (contentImportedAssets == null)
                    {
                        RefreshIContentImportAssets();
                    }

                    foreach (var loadedAsset in contentImportedAssets)
                    {
                        (loadedAsset as IContentImport)?.ReimportContent();
                    }

                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                });
            }
            else
            {
                CustomDebug.Log($"Can not load oobject {nameof(CommonContentSettings)}. " +
                                $"Access token won't be refreshed." +
                                $" Content won't be reimported ");
            }
        }


        [MenuItem("Drawmasters/Run game %g")]
        private static void RunGame() =>
            OpenScene(GameLayoutSettingsPath, GameScenePath);


        [MenuItem("Drawmasters/Run editor %e")]
        private static void RunEditor() =>
            OpenScene(EditorLayoutSettingsPath, EditorScenePath);
        
        
        [MenuItem("Drawmasters/Layout/Save Editor Layout")]
        private static void SaveEditorLayout() =>
            LayoutUtility.SaveLayoutToAsset(EditorLayoutSettingsPath);
        
        
        [MenuItem("Drawmasters/Layout/Save Game Layout")]
        private static void SaveGameLayout() =>
            LayoutUtility.SaveLayoutToAsset(GameLayoutSettingsPath);
        

        private static async void OpenScene(string layoutPath, string scenePath)
        {
            EditorApplication.isPlaying = false;

            await Task.Delay(100);

            LayoutUtility.LoadLayoutFromAsset(layoutPath);
            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = true;
        }

        #endregion
    }
}
