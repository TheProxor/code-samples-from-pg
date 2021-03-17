using UnityEditor;
using UnityEditor.UI;


namespace Drawmasters.LevelConstructor
{
    [CustomEditor(typeof(AlternativeKeyButton), true)]
    public class AlternativeKeyButtonEditor : ButtonEditor
    {
        #region Fields
        
        SerializedProperty buttonPressKeysProperty = null;

        #endregion



        #region Unity lifecycle

        void Awake()
        {
            buttonPressKeysProperty = serializedObject.FindProperty("buttonPressKeys");
        }

        #endregion
    
    
    
        #region OnGUI

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        
            EditorGUILayout.Space();
 
            serializedObject.Update();

            EditorGUILayout.PropertyField(buttonPressKeysProperty, true);
        
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
