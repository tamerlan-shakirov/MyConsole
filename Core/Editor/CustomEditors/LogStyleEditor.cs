/* ================================================================
   ---------------------------------------------------
   Project   :    MyConsole
   Publisher :    Renowned Games
   Developer :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using RenownedGames.ExLibEditor;
using RenownedGames.ExLibEditor.Windows;
using System;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    [CustomEditor(typeof(LogStyle))]
    sealed class LogStyleEditor : Editor
    {
        private SerializedProperty height;
        private SerializedProperty icon;
        private SerializedProperty background;
        private SerializedProperty color;
        private SerializedProperty font;
        private SerializedProperty fontSize;
        private SerializedProperty fontStyle;
        private SerializedProperty alignment;
        private SerializedProperty imagePosition;
        private SerializedProperty padding;

        private Texture textureIcon;
        private Texture refreshIcon;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            height = serializedObject.FindProperty("height");
            icon = serializedObject.FindProperty("icon");
            background = serializedObject.FindProperty("background");
            color = serializedObject.FindProperty("color");
            font = serializedObject.FindProperty("font");
            fontSize = serializedObject.FindProperty("fontSize");
            fontStyle = serializedObject.FindProperty("fontStyle");
            alignment = serializedObject.FindProperty("alignment");
            imagePosition = serializedObject.FindProperty("imagePosition");
            padding = serializedObject.FindProperty("padding");

            textureIcon = EditorGUIUtility.IconContent("PreTextureArrayFirstSlice").image;
            refreshIcon = EditorGUIUtility.IconContent("Refresh").image;
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            height.floatValue = EditorGUILayout.FloatField("Height", Mathf.Max(0, height.floatValue));

            GenericMenu menu = DefaultIconsMenu();
            MenuPropertyLayout("Icon", icon, textureIcon, menu.DropDown);

            background.objectReferenceValue = EditorGUILayout.ObjectField("Background", background.objectReferenceValue, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            color.colorValue = EditorGUILayout.ColorField("Color", color.colorValue);

            MenuPropertyLayout("Font", font, refreshIcon, (rect) =>
            {
                font.objectReferenceValue = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf");
                serializedObject.ApplyModifiedProperties();
            });

            fontSize.intValue = EditorGUILayout.IntField("Font Size", Mathf.Max(0, fontSize.intValue));
            fontStyle.enumValueIndex = EditorGUILayout.Popup("Font Style", fontStyle.enumValueIndex, fontStyle.enumDisplayNames);
            alignment.enumValueIndex = EditorGUILayout.Popup("Alignment", alignment.enumValueIndex, alignment.enumDisplayNames);
            imagePosition.enumValueIndex = EditorGUILayout.Popup("Image Position", imagePosition.enumValueIndex, imagePosition.enumDisplayNames);
            EditorGUILayout.PropertyField(padding);

            serializedObject.ApplyModifiedProperties();
        }

        private void MenuPropertyLayout(string name, SerializedProperty property, Texture icon, Action<Rect> onClick)
        {
            Rect filedPosition = GUILayoutUtility.GetRect(0, 20);

            filedPosition = EditorGUI.PrefixLabel(filedPosition, new GUIContent(name));
            filedPosition.x += 19;
            filedPosition.width -= 19;

            EditorGUI.PropertyField(filedPosition, property, GUIContent.none);

            Rect buttonPosition = new Rect(filedPosition.xMin - 19, filedPosition.y + 2, 15, 15);
            if (GUI.Button(buttonPosition, icon, "IconButton"))
            {
                onClick?.Invoke(buttonPosition);
            }
        }

        private GenericMenu DefaultIconsMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Info"), false, () => SetIconFunc(icon, "console.infoicon"));
            menu.AddItem(new GUIContent("Info Small"), false, () => SetIconFunc(icon, "console.infoicon.sml"));
            menu.AddItem(new GUIContent("Info Inactive"), false, () => SetIconFunc(icon, "console.infoicon.inactive.sml@2x"));
            menu.AddItem(new GUIContent("Info Inactive Small"), false, () => SetIconFunc(icon, "console.infoicon.inactive.sml"));

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Warning"), false, () => SetIconFunc(icon, "console.warnicon"));
            menu.AddItem(new GUIContent("Warning Small"), false, () => SetIconFunc(icon, "console.warnicon.sml"));
            menu.AddItem(new GUIContent("Warning Inactive"), false, () => SetIconFunc(icon, "console.warnicon.inactive.sml@2x"));
            menu.AddItem(new GUIContent("Warning Inactive Small"), false, () => SetIconFunc(icon, "console.warnicon.inactive.sml"));

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Error"), false, () => SetIconFunc(icon, "console.erroricon"));
            menu.AddItem(new GUIContent("Error Small"), false, () => SetIconFunc(icon, "console.erroricon.sml"));
            menu.AddItem(new GUIContent("Error Inactive"), false, () => SetIconFunc(icon, "console.erroricon.inactive.sml@2x"));
            menu.AddItem(new GUIContent("Error Inactive Small"), false, () => SetIconFunc(icon, "console.erroricon.inactive.sml"));
            return menu;
        }

        private void SetIconFunc(SerializedProperty property, string iconName)
        {
            property.objectReferenceValue = EditorGUIUtility.IconContent(iconName).image;
            property.objectReferenceValue.name = "Console Icon";
            serializedObject.ApplyModifiedProperties();
        }
    }
}