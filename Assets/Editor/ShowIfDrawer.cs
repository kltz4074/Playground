using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        SerializedProperty boolProperty =
            property.serializedObject.FindProperty(showIf.boolFieldName);

        if (boolProperty != null && boolProperty.propertyType == SerializedPropertyType.Boolean)
        {
            if (boolProperty.boolValue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        SerializedProperty boolProperty =
            property.serializedObject.FindProperty(showIf.boolFieldName);

        if (boolProperty != null &&
            boolProperty.propertyType == SerializedPropertyType.Boolean &&
            boolProperty.boolValue)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return 0f;
    }
}