using Drawmasters.Utils.Ui;
using UnityEditor;
using UnityEditor.UI;


namespace Drawmasters.Editor.Utils.Ui
{
    [CustomEditor(typeof(BlendImage))]
    public class BlendImageEditor : ImageEditor
    {
        private SerializedProperty blendFactorProperty;
        private SerializedProperty colorChangeRenderersProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            blendFactorProperty = serializedObject.FindProperty("blendFactor");
            colorChangeRenderersProperty = serializedObject.FindProperty("colorChangeRenderers");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(colorChangeRenderersProperty);

            EditorGUILayout.SelectableLabel("Blend Factor (Editor test only):", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(blendFactorProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
