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
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    [CustomEditor(typeof(ConsoleSettings))]
    sealed class ConsoleSettingsEditor : Editor
    {
        private SerializedProperty title;
        private SerializedProperty showTime;
        private SerializedProperty timeFormat;
        private SerializedProperty infoStyle;
        private SerializedProperty warningStyle;
        private SerializedProperty errorStyle;
        private SerializedProperty assertStyle;
        private SerializedProperty exceptionStyle;
        private SerializedProperty logEvenStyle;
        private SerializedProperty logOddStyle;
        private SerializedProperty stackTraceStyle;
        private SerializedProperty collapseCountStyle;
        private SerializedProperty consoleExporter;

        // Stored required properties.
        private bool showTitleField;
        private bool titleFreeMode;
        private bool consoleLabelUnlocked;
        private Texture searchIcon;
        private Texture guiIcon;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            title = serializedObject.FindProperty("title");
            showTime = serializedObject.FindProperty("showLogTime");
            timeFormat = serializedObject.FindProperty("logTimeFormat");
            infoStyle = serializedObject.FindProperty("infoStyle");
            warningStyle = serializedObject.FindProperty("warningStyle");
            errorStyle = serializedObject.FindProperty("errorStyle");
            assertStyle = serializedObject.FindProperty("assertStyle");
            exceptionStyle = serializedObject.FindProperty("exceptionStyle");
            logEvenStyle = serializedObject.FindProperty("logEvenStyle");
            logOddStyle = serializedObject.FindProperty("logOddStyle");
            stackTraceStyle = serializedObject.FindProperty("stackTraceStyle");
            collapseCountStyle = serializedObject.FindProperty("collapseCountStyle");
            consoleExporter = serializedObject.FindProperty("consoleExporter");

            searchIcon = EditorGUIUtility.IconContent("Search Icon").image;
            guiIcon = EditorGUIUtility.IconContent("GUISkin Icon").image;
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            TitleFieldLayout();

            showTime.boolValue = EditorGUILayout.Toggle("Show Time", showTime.boolValue);
            if (showTime.boolValue)
            {
                EditorGUI.indentLevel++;
                timeFormat.stringValue = EditorGUILayout.DelayedTextField("Format", timeFormat.stringValue);
                if (string.IsNullOrEmpty(timeFormat.stringValue))
                {
                    timeFormat.stringValue = "[hh:mm:ss]";
                }
                EditorGUI.indentLevel--;
            }

            infoStyle.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(infoStyle.isExpanded, new GUIContent("Log Styles"));
            if (infoStyle.isExpanded)
            {
                if (!EditorGUIUtility.hierarchyMode)
                {
                    EditorGUI.indentLevel++;
                }
                EditorGUILayout.PropertyField(infoStyle, new GUIContent("Info"));
                EditorGUILayout.PropertyField(warningStyle, new GUIContent("Warning"));
                EditorGUILayout.PropertyField(errorStyle, new GUIContent("Error"));
                EditorGUILayout.PropertyField(assertStyle, new GUIContent("Assert"));
                EditorGUILayout.PropertyField(exceptionStyle, new GUIContent("Exception"));
                if (!EditorGUIUtility.hierarchyMode)
                {
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            logEvenStyle.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(logEvenStyle.isExpanded, new GUIContent("Console Styles"));
            if (logEvenStyle.isExpanded)
            {
                if (!EditorGUIUtility.hierarchyMode)
                {
                    EditorGUI.indentLevel++;
                }
                TextStyleLayout("Log Even", logEvenStyle, "CN EntryBackEven");
                TextStyleLayout("Log Odd", logOddStyle, "CN EntryBackodd");
                TextStyleLayout("Stack Trace", stackTraceStyle, "CN Message");
                TextStyleLayout("Collapse Count", collapseCountStyle, "CN CountBadge");
                if (!EditorGUIUtility.hierarchyMode)
                {
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            consoleExporter.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(consoleExporter.isExpanded, new GUIContent("Exporter Settings"));
            if (consoleExporter.isExpanded)
            {
                if (!EditorGUIUtility.hierarchyMode)
                {
                    EditorGUI.indentLevel++;
                }
                foreach (SerializedProperty property in consoleExporter.GetVisibleChildren())
                {
                    EditorGUILayout.PropertyField(property);
                }
                if (!EditorGUIUtility.hierarchyMode)
                {
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void TitleFieldLayout()
        {
            Event current = Event.current;
            if (!showTitleField)
            {
                float space = EditorGUIUtility.labelWidth + 40;
                Rect clickPosition = new Rect(space, 5, EditorGUIUtility.currentViewWidth - space - 7, 15);

                if (current.alt
                    && current.control
                    && current.shift
                    && current.type == EventType.MouseDown
                    && current.button == 0
                    && clickPosition.Contains(current.mousePosition))
                {
                    showTitleField = true;
                    GUI.FocusControl(string.Empty);
                    GUIUtility.ExitGUI();
                    return;
                }
            }
            else
            {
                if (titleFreeMode)
                {
                    title.stringValue = EditorGUILayout.DelayedTextField("Window Title", title.stringValue);
                }
                else
                {
                    Rect titlePosition = GUILayoutUtility.GetRect(0, 18);
                    titlePosition.height = EditorGUIUtility.singleLineHeight;
                    titlePosition = EditorGUI.PrefixLabel(titlePosition, new GUIContent("Window Title"));
                    if (GUI.Button(titlePosition, title.stringValue, EditorStyles.popup))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("MyConsole"), false, SetTitleName, "MyConsole");

                        if (current.control && !current.alt && !current.shift)
                        {
                            consoleLabelUnlocked = true;
                            GUI.FocusControl(string.Empty);
                            GUIUtility.ExitGUI();
                            return;
                        }

                        if (consoleLabelUnlocked) 
                        {
                            menu.AddItem(new GUIContent("Console"), false, SetTitleName, "Console");
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Console (Lock)"));
                        }

                        if (consoleLabelUnlocked
                           && current.control
                           && current.alt
                           && current.shift)
                        {
                            titleFreeMode = true;
                            GUI.FocusControl(string.Empty);
                            GUIUtility.ExitGUI();
                            return;
                        }

                        menu.DropDown(titlePosition);
                    }
                }
            }
        }

        private void SetTitleName(object name)
        {
            title.stringValue = (string)name;
            serializedObject.ApplyModifiedProperties();
        }

        private void TextStyleLayout(string name, SerializedProperty property, string defaultStyle)
        {
            Rect position = GUILayoutUtility.GetRect(0, 20);
            position.height = EditorGUIUtility.singleLineHeight;
            position = EditorGUI.PrefixLabel(position, new GUIContent(name));
            position.width -= 20;

            EditorGUI.BeginChangeCheck();
            property.stringValue = EditorGUI.DelayedTextField(position, property.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                GUIStyle style = GUI.skin.FindStyle(property.stringValue);
                if (style == null)
                {
                    property.stringValue = defaultStyle;
                }
            }

            Rect buttonPosition = new Rect(position.xMax + 2, position.y + 1, 15, 15);
            if (GUI.Button(buttonPosition, searchIcon, "IconButton"))
            {
                ExSearchWindow searchWindow = ExSearchWindow.Create();

                searchWindow.AddEntry(new GUIContent("Box", guiIcon), "Box", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("Label", guiIcon), "Label", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("Button", guiIcon), "Button", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("Log Even", guiIcon), "CN EntryBackEven", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("Log Odd", guiIcon), "CN EntryBackodd", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("StackTrace Box", guiIcon), "CN Box", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("StackTrace Text", guiIcon), "CN Message", (name) => SetStyleName(property, name));
                searchWindow.AddEntry(new GUIContent("Collapse Count", guiIcon), "CN CountBadge", (name) => SetStyleName(property, name));

                GUIStyle[] styles = GUI.skin.customStyles;
                for (int i = 0; i < styles.Length; i++)
                {
                    GUIStyle style = styles[i];
                    searchWindow.AddEntry(new GUIContent(style.name, guiIcon), style.name, (name) => SetStyleName(property, name));
                }

                searchWindow.Open(position);
            }
        }

        private void SetStyleName(SerializedProperty property, object name)
        {
            property.stringValue = (string)name;
            serializedObject.ApplyModifiedProperties();
        }
    }
}