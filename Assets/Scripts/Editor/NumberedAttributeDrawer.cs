using UnityEditor;
using UnityEngine;


namespace Drawmasters
{
    [CustomPropertyDrawer(typeof(NumberedAttribute))]
    public class NumberedAttributeDrawer : PropertyDrawer
    {
        #region Fields

        const float TextOffset = 20f;

        const string DefaultElementName = "Number";

        private static readonly GUIStyle LabelHeaderStyle = new GUIStyle
                                                            {
                                                                alignment = TextAnchor.UpperLeft,
                                                                fontStyle = FontStyle.Bold,
                                                                stretchWidth = true,
                                                                fixedHeight = TextOffset
                                                            };

        #endregion



        #region Properties

        public string LabelName
        {
            get
            {
                string result = DefaultElementName;

                if (attribute is NumberedAttribute numbered &&
                    !string.IsNullOrEmpty(numbered.ElementName))
                {
                    result = numbered.ElementName;
                }

                return result;
            }
        }



        #endregion



        #region Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => base.GetPropertyHeight(property, label) + TextOffset;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect objectFieldRect = new Rect(position.x, position.y, position.width, position.height - TextOffset);

            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = EditorGUI.TextField(objectFieldRect, property.stringValue);
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.ObjectField(objectFieldRect, property);
            }
            else
            {
                //EditorGUI.ObjectField(objectFieldRect, property);
            }
                        
            int index = CommonUtility.LastElementIndex(property.propertyPath);

            Rect customTextRect = new Rect(position.x, position.y + TextOffset, position.width, position.height);

            EditorGUI.LabelField(customTextRect, $"{LabelName} : {index + 1}", LabelHeaderStyle);
        }

        #endregion
    }
}
