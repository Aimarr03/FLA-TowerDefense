#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TowerAttackBehaviour), true)]
public class TowerAttackBehaviourDrawer: PropertyDrawer
{
    static Dictionary<string, Type> typeMap;

    static void BuildTypeMap()
    {
        if (typeMap != null) return;

        var baseType = typeof(TowerAttackBehaviour);
        typeMap = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Type.EmptyTypes; }
            })
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .ToDictionary(
                t => ObjectNames.NicifyVariableName(t.Name),
                t => t
            );
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        BuildTypeMap();
        EditorGUI.BeginProperty(position, label, property);

        float y = position.y;
        float line = EditorGUIUtility.singleLineHeight;

        // Header box
        Rect boxRect = new Rect(position.x, y, position.width, line);
        GUI.Box(boxRect, GUIContent.none);

        // Label
        Rect labelRect = new Rect(position.x + 6, y, position.width * 0.5f, line);
        EditorGUI.LabelField(labelRect, "Attack Modifier");

        // Dropdown
        string typeName = property.managedReferenceFullTypename;
        string displayName = GetShortTypeName(typeName) ?? "Select Attack Modifier";

        Rect dropdownRect = new Rect(
            position.x + position.width * 0.5f,
            y,
            position.width * 0.5f - 6,
            line
        );

        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayName), FocusType.Passive))
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Empty"), property.managedReferenceValue == null, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            foreach (var kvp in typeMap)
            {
                var name = kvp.Key;
                var type = kvp.Value;

                menu.AddItem(
                    new GUIContent(name),
                    type.FullName == typeName,
                    () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                );
            }

            menu.ShowAsContext();
        }

        y += line + EditorGUIUtility.standardVerticalSpacing;

        // Draw child fields
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;

            SerializedProperty it = property.Copy();
            SerializedProperty end = it.GetEndProperty();

            it.NextVisible(true);
            while (!SerializedProperty.EqualContents(it, end))
            {
                float h = EditorGUI.GetPropertyHeight(it, true);
                Rect r = new Rect(position.x, y, position.width, h);
                EditorGUI.PropertyField(r, it, true);
                y += h + EditorGUIUtility.standardVerticalSpacing;
                it.NextVisible(false);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight
                     + EditorGUIUtility.standardVerticalSpacing;

        if (property.managedReferenceValue == null)
            return height;

        SerializedProperty it = property.Copy();
        SerializedProperty end = it.GetEndProperty();

        it.NextVisible(true);
        while (!SerializedProperty.EqualContents(it, end))
        {
            height += EditorGUI.GetPropertyHeight(it, true)
                    + EditorGUIUtility.standardVerticalSpacing;
            it.NextVisible(false);
        }

        return height;
    }

    static string GetShortTypeName(string full)
    {
        if (string.IsNullOrEmpty(full)) return null;
        var parts = full.Split(' ');
        return parts.Length > 1 ? parts[1].Split('.').Last() : full;
    }
}


#endif