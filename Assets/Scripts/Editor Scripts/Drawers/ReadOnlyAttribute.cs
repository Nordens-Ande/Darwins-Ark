#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

//This class (attribute) allows developers to convert either a SerializeField or public field into a readonly field inside of the Unity Editors inspector.
//Meaning the developer cannot change the value of the specific field, they can only view it.
//This attribute/drawer works perfectly for values that should be viewable (debug values etc) but not changeable, the attribute helps communicate this.
//Use it like this: '[ReadOnly] public ..' or '[SerializeField, ReadOnly] private ..'

/// <summary>
/// Configures a field to be displayed as read-only in the Unity Inspector. 
/// </summary>
/// 
/// <remarks>
/// The field can still be modified through code at runtime.
/// This attribute only affects how the field is presented in the Inspector and hinders modification through the Inspector.
/// 
/// <example>   
/// public fields:
/// <code>
/// [ReadOnly] public float speed;
/// </code>
/// </example>
/// 
/// <example> 
/// private fields:
/// <code>
/// [SerializeField, ReadOnly] private float speed;
/// </code>
/// </example>
public class ReadOnlyAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif