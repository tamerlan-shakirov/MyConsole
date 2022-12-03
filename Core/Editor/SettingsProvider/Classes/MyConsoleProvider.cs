/* ================================================================
   ---------------------------------------------------
   Project   :    MyConsole
   Publisher :    Renowned Games
   Developer :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    sealed class MyConsoleProvider : SettingsProvider
    {
        private ConsoleSettings settings;
        private ConsoleSettingsEditor editor;

        /// <summary>
        /// MyConsoleProvider constructor.
        /// </summary>
        /// <param name="path">Path used to place the SettingsProvider in the tree view of the Settings window. The path should be unique among all other settings paths and should use "/" as its separator.</param>
        /// <param name="scopes">Scope of the SettingsProvider. The Scope determines whether the SettingsProvider appears in the Preferences window (SettingsScope.User) or the Settings window (SettingsScope.Project).</param>
        /// <param name="keywords">List of keywords to compare against what the user is searching for. When the user enters values in the search box on the Settings window, SettingsProvider.HasSearchInterest tries to match those keywords to this list.</param>
        public MyConsoleProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) { }

        /// <summary>
        /// Use this function to implement a handler for when the user clicks on the Settings in the Settings window. You can fetch a settings Asset or set up UIElements UI from this function.
        /// </summary>
        /// <param name="searchContext">Search context in the search box on the Settings window.</param>
        /// <param name="rootElement">Root of the UIElements tree. If you add to this root, the SettingsProvider uses UIElements instead of calling SettingsProvider.OnGUI to build the UI. If you do not add to this VisualElement, then you must use the IMGUI to build the UI.</param>
        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            settings = ConsoleSettings.GetCurrent();
            editor = (ConsoleSettingsEditor)Editor.CreateEditor(settings, typeof(ConsoleSettingsEditor));
        }

        /// <summary>
        /// Use this function to draw the UI based on IMGUI. This assumes you haven't added any children to the rootElement passed to the OnActivate function.
        /// </summary>
        /// <param name="searchContext">Search context for the Settings window. Used to show or hide relevant properties.</param>
        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                {
                    float labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 248;
                    editor.OnInspectorGUI();
                    EditorGUIUtility.labelWidth = labelWidth;
                }
                GUILayout.EndVertical();
                GUILayout.Space(3);
            }
            GUILayout.EndHorizontal();

        }

        /// <summary>
        /// Register MyConsoleProvider in Project Settings window.
        /// </summary>
        /// <returns>New MyConsoleProvider instance.</returns>
        [SettingsProvider]
        public static SettingsProvider RegisterAITreeProvider()
        {
            return new MyConsoleProvider("Renowned Games/MyConsole", SettingsScope.Project);
        }

    }
}