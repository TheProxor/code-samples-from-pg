using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using I2.Loc;

public class SetLanguageUtility : EditorWindow
{

	private string currentLanguage;

    [MenuItem("Tools/I2 Localization/Set Language Utility")]
    private static void Setup()
    {
        InitializeWindow();
    }

    private static void InitializeWindow()
    {
        SetLanguageUtility window = CreateWindow<SetLanguageUtility>();

        window.titleContent.text = nameof(SetLanguageUtility);

        int width = Screen.currentResolution.width / 6;
        int height = Screen.currentResolution.height / 7;

        int x = Screen.currentResolution.width / 2 - width / 2;
        int y = Screen.currentResolution.height / 2 - height / 2;

        window.position = new Rect(x, y, width, height);

        window.Show();
    }

	private void OnGUI()
	{
		LocalizationManager.UpdateSources();
		string[] Languages = LocalizationManager.GetAllLanguages().ToArray();
		System.Array.Sort(Languages);

		int index = System.Array.IndexOf(Languages, currentLanguage);

		GUI.changed = false;
		index = EditorGUILayout.Popup("Language", index, Languages);
		if (GUI.changed)
		{
			if (index < 0 || index >= Languages.Length)
			{
				currentLanguage = string.Empty;
			}
			else
			{
				currentLanguage = Languages[index];
			}

			GUI.changed = false;
		}

		GUILayout.Space(5);

		if (GUILayout.Button("Apply") && LocalizationManager.HasLanguage(currentLanguage))
		{
			LocalizationManager.CurrentLanguage = currentLanguage;
		}
	}
}
