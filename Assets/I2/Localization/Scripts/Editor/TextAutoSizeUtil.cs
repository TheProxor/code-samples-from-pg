using UnityEngine;
using UnityEditor;
using TMPro;

namespace I2.Loc
{
    public class TextAutoSizeUtil : EditorWindow
    {
        #region Fields

        private GameObject prefab;

        #endregion



        #region Methods

        [MenuItem("Tools/I2 Localization/Text Auto Size Util")]
        private static void Setup()
        {
            InitializeWindow();
        }


        private static void InitializeWindow()
        {
            TextAutoSizeUtil window = CreateWindow<TextAutoSizeUtil>();

            window.titleContent.text = nameof(TextAutoSizeUtil);

            int width = Screen.currentResolution.width / 6;
            int height = Screen.currentResolution.height / 4;

            int x = Screen.currentResolution.width / 2 - width / 2;
            int y = Screen.currentResolution.height / 2 - height / 2;

            window.position = new Rect(x, y, width, height);

            window.Show();
        }


        private void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Space(5);

                prefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), allowSceneObjects: true);

                GUILayout.Space(5);

                if (GUILayout.Button("Start"))
                {
                    SetTextSize();
                }
            }
            GUILayout.EndVertical();
        }

        private void SetTextSize()
        {
            TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>();

            foreach (var text in texts)
            {
                if (text.GetComponent<Localize>() == null)
                {
                    continue;
                }

                float fontSize = text.fontSize;
                text.enableAutoSizing = true;
                text.fontSizeMax = fontSize;
            }

            EditorUtility.SetDirty(prefab);
        }

        #endregion
    }
}