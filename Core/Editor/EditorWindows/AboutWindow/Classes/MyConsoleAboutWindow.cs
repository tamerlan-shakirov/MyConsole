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
using UnityEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    public sealed class MyConsoleAboutWindow : AboutWindow
    {
        /// <summary>
        /// Implement this method to add project name.
        /// </summary>
        protected override void InitializeProjectName(out string projectName)
        {
            projectName = "MyConsole";
        }

        /// <summary>
        /// Implement this method to add version label.
        /// </summary>
        protected override void InitializeVersion(out string version)
        {
            version = "Version: 2.0.0";
        }

        /// <summary>
        /// Implement this method to add all the people involved in the development.
        /// </summary>
        protected override void InitializeDevelopers(out Developer[] developers)
        {
            developers = new Developer[2]
            {
                new Developer("Publisher: ", "Renowned Games"),
                new Developer("Lead Developer: ", "Tamerlan Shakirov")
            };
        }

        /// <summary>
        /// Implement this method to add logotype.
        /// </summary>
        public override void InitializeLogotype(out Texture2D texture, out float width, out float height)
        {
            texture = EditorResources.Load<Texture2D>("Images/Logotype/MyConsole_420x280.png");
            width = 140;
            height = 93.5f;
        }

        /// <summary>
        /// Implement this method to add copyright.
        /// </summary>
        protected override void InitializeCopyright(out string copyright)
        {
            copyright = "Copyright 2022 Renowned Games All rights reserved.";
        }

        /// <summary>
        /// Implement this method to add publisher link button.
        /// </summary>
        protected override void InitializePublisherLink(out string url)
        {
            url = "https://assetstore.unity.com/publishers/26774";
        }

        [MenuItem("Renowned Games/MyConsole/About", false, 80)]
        public static void Open()
        {
            Open<MyConsoleAboutWindow>(new GUIContent("MyConsole"), new Vector2(450, 150));
        }

    }
}

