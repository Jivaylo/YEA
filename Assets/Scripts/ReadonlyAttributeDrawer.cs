using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadonlyAttribute))]
public sealed class ReadonlyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ReadonlyAttribute _attribute = (ReadonlyAttribute)attribute;
        try
        {
            string text = label.text + ": ";

            switch (property.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                    if (_attribute.Reset)
                    {
                        var value = property.arraySize;
                        text += value;
                        var defaultValue = (int)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.arraySize = defaultValue;
                    }
                    else
                        text += property.boundsIntValue;

                    break;

                case SerializedPropertyType.Boolean:
                    if (_attribute.Reset)
                    {
                        var value = property.boolValue;
                        text += value;
                        var defaultValue = (bool)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.boolValue = defaultValue;
                    }
                    else
                        text += property.boolValue;

                    break;

                case SerializedPropertyType.Bounds:
                    if (_attribute.Reset)
                    {
                        var value = property.boundsValue;
                        text += value;
                        var defaultValue = (Bounds)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.boundsValue = defaultValue;
                    }
                    else
                        text += property.boundsValue;

                    break;

                case SerializedPropertyType.BoundsInt:
                    if (_attribute.Reset)
                    {
                        var value = property.boundsIntValue;
                        text += value;
                        var defaultValue = (BoundsInt)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.boundsIntValue = defaultValue;
                    }
                    else
                        text += property.boundsIntValue;

                    break;

                case SerializedPropertyType.Character:
                    if (_attribute.Reset)
                    {
                        var value = (char)property.intValue;
                        text += value;
                        var defaultValue = (char)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.intValue = defaultValue;
                    }
                    else
                        text += (char)property.intValue;

                    break;

                case SerializedPropertyType.Color:
                    if (_attribute.Reset)
                    {
                        var value = property.colorValue;
                        text += ColorUtility.ToHtmlStringRGBA(value);
                        var defaultValue = (Color)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.colorValue = defaultValue;
                    }
                    else
                        text += ColorUtility.ToHtmlStringRGBA(property.colorValue);

                    break;

                case SerializedPropertyType.Enum:
                    if (_attribute.Reset)
                    {
                        var value = property.enumValueIndex;
                        text += property.enumDisplayNames[value];
                        var defaultValue = (int)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.enumValueIndex = defaultValue;
                    }
                    else
                        text += property.enumDisplayNames[property.enumValueIndex];

                    break;

                case SerializedPropertyType.ExposedReference:
                    if (_attribute.Reset)
                    {
                        var value = property.exposedReferenceValue;
                        text += value ? value.name : "Null";
                        var defaultValue = (Object)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.exposedReferenceValue = defaultValue;
                    }
                    else
                        text += property.exposedReferenceValue ? property.exposedReferenceValue.name : "Null";

                    break;

                case SerializedPropertyType.FixedBufferSize:
                    // Cannot be written to, so reset is moot.
                    text += property.fixedBufferSize;
                    break;

                case SerializedPropertyType.Float:
                    if (_attribute.Reset)
                    {
                        var value = property.floatValue;
                        text += value;
                        var defaultValue = (float)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.floatValue = defaultValue;
                    }
                    else
                        text += property.floatValue;

                    break;

                case SerializedPropertyType.Integer:
                    if (_attribute.Reset)
                    {
                        var value = property.intValue;
                        text += value;
                        var defaultValue = (int)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.intValue = defaultValue;
                    }
                    else
                        text += property.intValue;

                    break;

                case SerializedPropertyType.LayerMask:
                    if (_attribute.Reset)
                    {
                        var value = property.intValue;
                        text += LayerMask.LayerToName(value);
                        var defaultValue = (int)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.intValue = defaultValue;
                    }
                    else
                        text += LayerMask.LayerToName(property.intValue);

                    break;

                case SerializedPropertyType.ObjectReference:
                    if (_attribute.Reset)
                    {
                        var value = property.objectReferenceValue;
                        text += value ? value.name : "Null";
                        var defaultValue = (Object)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.objectReferenceValue = defaultValue;
                    }
                    else
                        text += property.objectReferenceValue ? property.objectReferenceValue.name : "Null";

                    break;

                case SerializedPropertyType.Quaternion:
                    var quat = property.quaternionValue;
                    if (_attribute.Reset)
                    {
                        var defaultValue = (Quaternion)_attribute.defaultValue;

                        if (quat != defaultValue)
                            property.quaternionValue = defaultValue;
                    }
                    text += quat + " (euler: " + quat.eulerAngles + ")";

                    break;

                case SerializedPropertyType.Rect:
                    if (_attribute.Reset)
                    {
                        var value = property.rectValue;
                        text += value;
                        var defaultValue = (Rect)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.rectValue = defaultValue;
                    }
                    else
                        text += property.rectValue;

                    break;

                case SerializedPropertyType.RectInt:
                    if (_attribute.Reset)
                    {
                        var value = property.rectIntValue;
                        text += value;
                        var defaultValue = (RectInt)_attribute.defaultValue;

                        // For some bizarre reason, RectInt lacks the == and != operators.
                        if (value.Equals(defaultValue))
                            property.rectIntValue = defaultValue;
                    }
                    else
                        text += property.rectIntValue;

                    break;

                case SerializedPropertyType.String:
                    if (_attribute.Reset)
                    {
                        var value = property.stringValue;
                        text += value;
                        var defaultValue = (string)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.stringValue = defaultValue;
                    }
                    else
                        text += property.stringValue;

                    break;

                case SerializedPropertyType.Vector2:
                    if (_attribute.Reset)
                    {
                        var value = property.vector2Value;
                        text += value;
                        var defaultValue = (Vector2)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.vector2Value = defaultValue;
                    }
                    else
                        text += property.vector2Value;

                    break;

                case SerializedPropertyType.Vector2Int:
                    if (_attribute.Reset)
                    {
                        var value = property.vector2IntValue;
                        text += value;
                        var defaultValue = (Vector2Int)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.vector2IntValue = defaultValue;
                    }
                    else
                        text += property.vector2IntValue;

                    break;

                case SerializedPropertyType.Vector3:
                    if (_attribute.Reset)
                    {
                        var value = property.vector3Value;
                        text += value;
                        var defaultValue = (Vector3)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.vector3Value = defaultValue;
                    }
                    else
                        text += property.vector3Value;

                    break;

                case SerializedPropertyType.Vector3Int:
                    if (_attribute.Reset)
                    {
                        var value = property.vector3IntValue;
                        text += value;
                        var defaultValue = (Vector3Int)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.vector3IntValue = defaultValue;
                    }
                    else
                        text += property.vector3IntValue;

                    break;

                case SerializedPropertyType.Vector4:
                    if (_attribute.Reset)
                    {
                        var value = property.vector4Value;
                        text += value;
                        var defaultValue = (Vector4)_attribute.defaultValue;

                        if (value != defaultValue)
                            property.vector4Value = defaultValue;
                    }
                    else
                        text += property.vector4Value;

                    break;

                default:
                    EditorGUI.HelpBox(position, "Cannot use ReadonlyAttribute to draw a Serialized Property of type "
                        + property.propertyType.ToString(), MessageType.Error);
                    return;
            }

            EditorGUI.HelpBox(position, text, (MessageType)(int)_attribute.type);
        }
        catch (System.Exception e)
        {
            EditorGUI.HelpBox(position, e.Message, MessageType.Error);
        }
    }
}