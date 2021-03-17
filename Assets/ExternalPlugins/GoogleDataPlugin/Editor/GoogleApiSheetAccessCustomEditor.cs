using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ButtonAttribute = Google.ApiPlugin.GoogleApiSheetAccessButtonAttribute;


namespace Google.ApiPlugin.Sheets
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GoogleApiSheetAccessSettings), true)]
    public class ObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawButtons();
        }


        public void DrawButtons()
        {
            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetParameters().Length == 0);

            foreach (var method in methods)
            {
                var ba = (ButtonAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));

                if (ba != null)
                {
                    var wasEnabled = GUI.enabled;
                    GUI.enabled = true;

                    var buttonName = string.IsNullOrEmpty(ba.Name) ? ObjectNames.NicifyVariableName(method.Name) : ba.Name;
                    if (GUILayout.Button(buttonName))
                    {
                        foreach (var t in targets)
                        {
                            method.Invoke(t, null);
                        }
                    }

                    GUI.enabled = wasEnabled;
                }
            }
        }
    }
}