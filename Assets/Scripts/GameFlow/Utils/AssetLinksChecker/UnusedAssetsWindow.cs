using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace AssetsUtils
{

#if UNITY_EDITOR

    public class UnusedAssetsWindow : EditorWindow
    {
        #region Fields

        private static Vector2 scrollPos = Vector2.one * 100.0f;
        private static List<AssetInfo> info;

        #endregion



        #region Public methods

        public static void ShowResult(List<AssetInfo> _info = null)
        {
            info = _info;
            UnusedAssetsWindow window = (UnusedAssetsWindow)GetWindow(typeof(UnusedAssetsWindow));

            window.Show();
        }

        #endregion



        #region Private methods

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUILayout.LabelField("FOUND MISSED INFO:");

            foreach (var i in info)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("PATH: ");
                EditorGUILayout.TextField(i.Path);

                GUILayout.Space(10.0f);

                EditorGUILayout.PrefixLabel("GUID:");
                EditorGUILayout.TextField(i.Guid);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion
    }

#endif

}
