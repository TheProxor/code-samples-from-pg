using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace AssetsUtils
{

#if UNITY_EDITOR

    public class MissedAssetsLinksWindow : EditorWindow
    {
        #region Fields

        private static Vector2 scrollPos = Vector2.one * 100.0f;
        private static List<(string guid, string path)> info;

        #endregion



        #region Methods

        public static void ShowResult(List<(string guid, string path)> _info = null)
        {
            info = _info;
            MissedAssetsLinksWindow window = (MissedAssetsLinksWindow)GetWindow(typeof(MissedAssetsLinksWindow));

            window.Show();
        }

        #endregion



        #region Private methods

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.LabelField("FOUND MISSED ASSETS LINKS INFO:");

            foreach (var (guid, path) in info)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("IN ASSET WITH PATH: ");
                EditorGUILayout.TextField(path);

                GUILayout.Space(10.0f);

                EditorGUILayout.PrefixLabel("MISSED ASSET FOR GUID:");
                EditorGUILayout.TextField(guid);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion
    }

#endif

}
