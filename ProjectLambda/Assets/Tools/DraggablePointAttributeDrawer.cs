using System;
using System.Collections;
using System.Reflection;
using System.Linq;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class DraggablePointAttributeDrawer : Editor
{
    private readonly GUIStyle style = new GUIStyle();

    private Transform transform;

    private void OnEnable()
    {
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        transform = (target as MonoBehaviour).transform;
    }

    public void OnSceneGUI()
    {
        if (Application.isPlaying) return;
        SerializedProperty property = serializedObject.GetIterator();
        CheckProperty(property);
    }

    private void CheckProperty(SerializedProperty property)
    {
        while (property.Next(true))
        {
            if (property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector3)
            {
                FieldInfo field = GetParent(property).GetType().GetField(property.name);
                if (field == null) continue;
                object[] draggablePoints = field.GetCustomAttributes(typeof(DraggablePointAttribute), false);
                if (draggablePoints.Length > 0)
                {
                    DraggablePointAttribute attribute = draggablePoints[0] as DraggablePointAttribute;
                    DrawPoint(property, attribute.local, property.displayName);
                }
            }
            else if (property.propertyType == SerializedPropertyType.Generic && property.isArray)
            {
                FieldInfo field = GetParent(property).GetType().GetField(property.name);
                if (field == null) continue;
                object[] draggablePoints = field.GetCustomAttributes(typeof(DraggablePointAttribute), false);
                if (draggablePoints.Length > 0)
                {
                    DraggablePointAttribute attribute = draggablePoints[0] as DraggablePointAttribute;
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        SerializedProperty element = property.GetArrayElementAtIndex(i);
                        if (element.propertyType == SerializedPropertyType.Vector2 || element.propertyType == SerializedPropertyType.Vector3)
                        {
                            DrawPoint(element, attribute.local, property.displayName + " " + i);
                        }
                    }
                }
            }
        }
    }

    private void DrawPoint(SerializedProperty property, bool local, string name)
    {
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            Vector2 drawPos = local ? (Vector2)transform.TransformPoint(property.vector2Value) : property.vector2Value;
            Handles.Label(drawPos, name, style);
            EditorGUI.BeginChangeCheck();
            Vector2 position = Handles.PositionHandle(drawPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                if (local) position = transform.InverseTransformPoint(position);
                property.vector2Value = position;
                serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            Vector3 drawPos = local ? transform.TransformPoint(property.vector3Value) : property.vector3Value;
            Handles.Label(drawPos, name, style);
            EditorGUI.BeginChangeCheck();
            Vector3 position = Handles.PositionHandle(drawPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                if (local) position = transform.InverseTransformPoint(position);
                property.vector3Value = position;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public object GetParent(SerializedProperty prop)
    {
        string path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        string[] elements = path.Split('.');
        foreach (string element in elements.Take(elements.Length - 1))
        {
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj ?? prop.serializedObject.targetObject;
    }

    public object GetValue(object source, string name)
    {
        if (source == null)
            return null;
        Type type = source.GetType();
        FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null)
                return null;
            return p.GetValue(source, null);
        }
        return f.GetValue(source);
    }

    public object GetValue(object source, string name, int index)
    {
        IEnumerable enumerable = GetValue(source, name) as IEnumerable;
        IEnumerator enm = enumerable.GetEnumerator();
        while (index-- >= 0)
            enm.MoveNext();
        return enm.Current;
    }
}
