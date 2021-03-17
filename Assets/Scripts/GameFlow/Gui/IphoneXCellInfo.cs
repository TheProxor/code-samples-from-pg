using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Drawmasters
{
    [RequireComponent(typeof(LayoutElement))]
    public class IphoneXCellInfo : MonoBehaviour
    {
        [SerializeField] bool isMonobrowSize = default;

        [CustomValueDrawer("IPhoneXOffsetValue")]
        [SerializeField] float additionalHeight = default;
        [SerializeField] float additionalWidth = default;

        LayoutElement layoutElement;

        float? initialHeight;
        float? initialWidth;



        LayoutElement LayoutElement
        {
            get
            {
                if (layoutElement == null)
                {
                    layoutElement = GetComponent<LayoutElement>();
                }

                return layoutElement;
            }
        }


        public float IPhoneXOffsetValue(float currentValue, GUIContent guiContent)
        {
            float result = 0f;

            #if UNITY_EDITOR
                if (isMonobrowSize)
                {
                    currentValue = ResolutionUtility.CommonIPhoneXOffset;
                }

                EditorGUI.BeginDisabledGroup(isMonobrowSize);
                result = EditorGUILayout.FloatField(guiContent.text, currentValue);
                EditorGUI.EndDisabledGroup();
            #endif

            return result;
        }


        public void RecalculateValues()
        {
            initialHeight = initialHeight ?? LayoutElement.preferredHeight;
            initialWidth = initialWidth ?? LayoutElement.preferredWidth;

            LayoutElement.preferredHeight = initialHeight.Value + additionalHeight;
            LayoutElement.preferredWidth = initialWidth.Value + additionalWidth;
        }
    }
}
