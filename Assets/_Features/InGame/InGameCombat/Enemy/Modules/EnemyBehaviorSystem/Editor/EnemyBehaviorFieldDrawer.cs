using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyBehaviorFieldAttribute))]
public sealed class EnemyBehaviorFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnemyBehaviorFieldAttribute behaviorField = (EnemyBehaviorFieldAttribute)attribute;
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(
            position,
            property,
            new GUIContent(behaviorField.Label, label.tooltip),
            true);

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            int minimum = float.IsNegativeInfinity(behaviorField.Minimum)
                ? int.MinValue
                : Mathf.CeilToInt(behaviorField.Minimum);
            int maximum = float.IsPositiveInfinity(behaviorField.Maximum)
                ? int.MaxValue
                : Mathf.FloorToInt(behaviorField.Maximum);
            property.intValue = Mathf.Clamp(property.intValue, minimum, maximum);
        }
        else if (property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = Mathf.Clamp(
                property.floatValue,
                behaviorField.Minimum,
                behaviorField.Maximum);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnemyBehaviorFieldAttribute behaviorField = (EnemyBehaviorFieldAttribute)attribute;
        return EditorGUI.GetPropertyHeight(
            property,
            new GUIContent(behaviorField.Label, label.tooltip),
            true);
    }
}
