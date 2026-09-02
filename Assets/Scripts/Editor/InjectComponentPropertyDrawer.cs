#if UNITY_EDITOR
using MarioTest.Core;
using UnityEditor;
using UnityEngine;

namespace MarioTest.Editor
{
    [CustomPropertyDrawer(typeof(InjectComponentAttribute))]
    public sealed class InjectComponentPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Component owner = property.serializedObject.targetObject as Component;
            if (owner != null
                && property.propertyType == SerializedPropertyType.ObjectReference
                && property.objectReferenceValue == null)
            {
                Component component = owner.GetComponent(fieldInfo.FieldType);
                if (component != null)
                {
                    property.objectReferenceValue = component;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif
