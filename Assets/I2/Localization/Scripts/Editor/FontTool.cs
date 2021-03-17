using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace I2.Loc
{
    public class FontTool : EditorWindow
    {
        private string unicodeHexString = string.Empty;
        private string currentLanguage = string.Empty;
        private int currentLanguageIndex = 0;

        [MenuItem("Tools/I2 Localization/Font Tool")]
        private static void Setup()
        {
            InitializeWindow();
        }


        private static void InitializeWindow()
        {
            FontTool window = CreateWindow<FontTool>();

            window.titleContent.text = nameof(FontTool);

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

                LocalizationManager.UpdateSources();
                string[] Languages = LocalizationManager.GetAllLanguages().ToArray();
                Array.Sort(Languages);

                currentLanguageIndex = System.Array.IndexOf(Languages, currentLanguage);

                GUI.changed = false;
                currentLanguageIndex = EditorGUILayout.Popup("Language", currentLanguageIndex, Languages);
                if (GUI.changed)
                {
                    if (currentLanguageIndex < 0 || currentLanguageIndex >= Languages.Length)
                    {
                        currentLanguage = string.Empty;
                    }
                    else
                    {
                        currentLanguage = Languages[currentLanguageIndex];
                    }

                    GUI.changed = false;
                }


                GUILayout.Space(5);

                if (GUILayout.Button("Start"))
                {
                    ExtractUnicodeHex();
                }

                GUILayout.Space(15);

                if (unicodeHexString.Length > 0)
                {
                    GUILayout.TextArea(unicodeHexString);
                }
            }
            GUILayout.EndVertical();
        }


        private void ExtractUnicodeHex()
        {
            HashSet<int> codes = new HashSet<int>();
            
            foreach(var source in LocalizationManager.Sources)
            { 
                int index = source.GetLanguageIndex(currentLanguage); 

                foreach (var term in source.mTerms)
                {
                    string text = term.Languages[index];

                    for (int i = 0; i < text.Length; i++)
                    {
                        codes.Add(text[i]);
                    }
                }
            }

            unicodeHexString = GetHexString(codes);
        }


        private string GetHexString(HashSet<int> codes)
        {
            string result = string.Empty;

            foreach (var code in codes)
            {
                result += code.ToString("X4") + ",";
            }

            result = result.Remove(result.Length - 1, 1);

            return result;
        }
    }
}
