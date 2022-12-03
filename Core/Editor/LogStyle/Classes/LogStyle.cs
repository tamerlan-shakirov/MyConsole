/* ================================================================
   ---------------------------------------------------
   Project   :    MyConsole
   Publisher :    Renowned Games
   Developer :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using UnityEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    [CreateAssetMenu(fileName = "Log Style", menuName = "Renowned Games/MyConsole/Log Style")]
    public class LogStyle : ScriptableObject
    {
        [SerializeField]
        private float height;

        [SerializeField]
        private Texture icon;

        [SerializeField]
        private Texture2D background;

        [SerializeField]
        private Color color;

        [SerializeField]
        private Font font;

        [SerializeField]
        private int fontSize;

        [SerializeField]
        private FontStyle fontStyle;

        [SerializeField]
        private TextAnchor alignment;

        [SerializeField]
        private ImagePosition imagePosition;

        [SerializeField]
        private RectOffset padding;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
            if(font == null)
            {
                font = (Font)EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf");
            }
        }

        /// <summary>
        /// Create GUIStyle from log settings.
        /// </summary>
        public virtual GUIStyle CreateGUIStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.background = background;
            style.normal.textColor = color;
            style.active.background = background;
            style.active.textColor = color;
            style.hover.background = background;
            style.hover.textColor = color;
            style.focused.background = background;
            style.focused.textColor = color;
            style.font = font;
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            style.alignment = alignment;
            style.imagePosition = imagePosition;
            style.padding = padding;
            style.richText = true;
            return style;
        }

        /// <summary>
        /// Called when the user hits the Reset button in the Inspector's context menu
        /// or when adding the component the first time.
        /// </summary>
        protected virtual void Reset()
        {
            height = 22;
            color = new Color(0.78f, 0.78f, 0.78f, 1.0f);
            font = (Font)EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf");
            fontSize = 12;
            fontStyle = FontStyle.Normal;
            alignment = TextAnchor.MiddleLeft;
            imagePosition = ImagePosition.ImageLeft;
            padding = new RectOffset(5, 0, 0, 0);
        }

        #region [Static Methods]
        public static LogStyle CreateDefault(LogType logType)
        {
            LogStyle style = CreateInstance<LogStyle>();
            style.name = $"Log style {logType}";
            style.Reset();
            switch (logType)
            {
                case LogType.Log:
                    style.icon = EditorGUIUtility.IconContent("console.infoicon.sml").image;
                    break;
                case LogType.Warning:
                    style.icon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    style.icon = EditorGUIUtility.IconContent("console.erroricon.sml").image;
                    break;
            }
            return style;
        }
        #endregion

        #region [Getter / Setter]
        public float GetHeight()
        {
            return height;
        }

        public void SetHeight(float value)
        {
            height = value;
        }

        public Texture GetIcon()
        {
            return icon;
        }

        public void SetIcon(Texture value)
        {
            icon = value;
        }

        public Texture GetBackground()
        {
            return background;
        }

        public void SetBackground(Texture2D value)
        {
            background = value;
        }

        public Color GetColor()
        {
            return color;
        }

        public void SetColor(Color value)
        {
            color = value;
        }

        public Font GetFont()
        {
            return font;
        }

        public void SetFont(Font value)
        {
            font = value;
        }

        public int GetFontSize()
        {
            return fontSize;
        }

        public void SetFontSize(int value)
        {
            fontSize = value;
        }

        public FontStyle GetFontStyle()
        {
            return fontStyle;
        }

        public void SetFontStyle(FontStyle value)
        {
            fontStyle = value;
        }

        public TextAnchor GetAlignment()
        {
            return alignment;
        }

        public void SetAlignment(TextAnchor value)
        {
            alignment = value;
        }

        public ImagePosition GetImagePosition()
        {
            return imagePosition;
        }

        public void SetImagePosition(ImagePosition value)
        {
            imagePosition = value;
        }

        public RectOffset GetPadding()
        {
            return padding;
        }

        public void SetPadding(RectOffset value)
        {
            padding = value;
        }
        #endregion
    }
}