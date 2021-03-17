using System;
using UnityEditor;
using UnityEngine;


namespace Core
{
    [CustomPropertyDrawer(typeof(LinkScene))]
    public class LinkSceneDrawer : PropertyDrawer
    {
        private const string NamePropertyPath = "path";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            string[] values = AssetDatabase.FindAssets("t:Scene");
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = AssetDatabase.GUIDToAssetPath(values[i]);
            }

            if(_i < 0)
            {
                string prevPath = property.FindPropertyRelative(NamePropertyPath).stringValue;
                _i = Mathf.Max(Array.IndexOf(values, prevPath), 0);
            }

            _i = EditorGUI.Popup(position, _i, values);
            property.FindPropertyRelative(NamePropertyPath).stringValue = values[_i];
        }

        private int _i = -1;
    }
}
