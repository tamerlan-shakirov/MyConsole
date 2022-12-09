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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    public sealed class MyConsoleWindow : EditorWindow, IHasCustomMenu
    {
        [Flags]
        public enum ClearMode
        {
            None = 0,
            OnPlay = 1 << 0,
            OnBuild = 1 << 1,
            OnRecompile = 1 << 2,
            All = ~0
        }

        [Flags]
        public enum FilterMode
        {
            None = 0,
            Info = 1 << 0,
            Assert = 1 << 1,
            Warning = 1 << 2,
            Error = 1 << 3,
            Exception = 1 << 4,
            All = ~0
        }

        [SerializeField]
        private List<Log> logs = new List<Log>();

        [SerializeField]
        private ClearMode clearMode = ClearMode.OnPlay;

        [SerializeField]
        private FilterMode filterMode = FilterMode.All;

        [SerializeField]
        private bool collapse;

        [SerializeField]
        private bool errorPause;

        [SerializeField]
        private string searchText;

        [SerializeField]
        private int selected;

        [SerializeField]
        private string stackTraceHyperText;

        // Stored required properties.
        private Dictionary<Log, int> collapseLogs;
        private ConsoleSettings settings;
        private GUISplitView splitView;
        private Texture infoIcon;
        private Texture warningIcon;
        private Texture errorIcon;
        private GUIStyle infoStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;
        private GUIStyle assertStyle;
        private GUIStyle exceptionStyle;
        private GUIStyle logEvenStyle;
        private GUIStyle logOddStyle;
        private GUIStyle stackTraceStyle;
        private GUIStyle collapseCountStyle;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            settings = ConsoleSettings.GetCurrent();
            infoIcon = EditorGUIUtility.IconContent("console.infoicon.sml").image;
            warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
            errorIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image;
            splitView = new GUISplitView(GUISplitView.Direction.Vertical, 0.85f, 0.15f, 0.85f);
            splitView.SetOffset(20);

            selected = -1;
            searchText = string.Empty;
            if (logs.Count == 0)
            {
                stackTraceHyperText = string.Empty;
            }

            Application.logMessageReceivedThreaded += AddLog;
            Application.logMessageReceivedThreaded += ErrorPause;
            EditorApplication.playModeStateChanged += ClearOnModeChanged;
        }

        /// <summary>
        /// Called when the window gets keyboard focus.
        /// </summary>
        private void OnFocus()
        {
            collapseLogs = new Dictionary<Log, int>();
            for (int i = 0; i < logs.Count; i++)
            {
                AddCollapseLog(logs[i]);
            }

            infoStyle = null;
            warningStyle = null;
            errorStyle = null;
            assertStyle = null;
            exceptionStyle = null;
        }

        /// <summary>
        /// Called when the console becomes disabled.
        /// </summary>
        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= AddLog;
            Application.logMessageReceivedThreaded += ErrorPause;
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        private void OnGUI()
        {
            InitializeGUIStyles();
            OnToolbarGUI();
            OnLogsGUI();
        }

        /// <summary>
        /// Called once to initialize required GUI styles.
        /// </summary>
        private void InitializeGUIStyles()
        {
            if (infoStyle == null)
            {
                infoStyle = settings.GetInfoStyle().CreateGUIStyle();
                warningStyle = settings.GetWarningStyle().CreateGUIStyle();
                errorStyle = settings.GetErrorStyle().CreateGUIStyle();
                assertStyle = settings.GetAssertStyle().CreateGUIStyle();
                exceptionStyle = settings.GetExceptionStyle().CreateGUIStyle();

                logEvenStyle = new GUIStyle(settings.GetLogEvenStyle());
                logOddStyle = new GUIStyle(settings.GetLogOddStyle());
                stackTraceStyle = new GUIStyle(settings.GetStackTraceStyle());
                collapseCountStyle = new GUIStyle(settings.GetCollapseCountStyle());

            }
        }

        /// <summary>
        /// Called every GUI call to draw toolbar.
        /// </summary>
        private void OnToolbarGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45)))
                {
                    Clear();
                }

                clearMode = (ClearMode)EditorGUILayout.EnumFlagsField(GUIContent.none, clearMode, EditorStyles.toolbarDropDown, GUILayout.Width(17));

                Rect collapsePosition = GUILayoutUtility.GetRect(60, 0);
                collapsePosition.width = 60;
                collapsePosition.height = 20;
                collapse = EditorGUI.Toggle(collapsePosition, collapse, EditorStyles.toolbarButton);
                collapsePosition.x += 2;
                GUI.Label(collapsePosition, "Collapse");

                Rect errorPausePosition = GUILayoutUtility.GetRect(75, 0);
                errorPausePosition.width = 75;
                errorPausePosition.height = 20;
                errorPause = EditorGUI.Toggle(errorPausePosition, errorPause, EditorStyles.toolbarButton);
                errorPausePosition.x += 2;
                GUI.Label(errorPausePosition, "Error Pause");

                GUILayout.FlexibleSpace();

                searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField, GUILayout.Width(300));

                int infoCount = 0;
                int warningCount = 0;
                int errorCount = 0;
                foreach (KeyValuePair<Log, int> item in collapseLogs)
                {
                    Log log = item.Key;
                    switch (log.GetLogType())
                    {
                        case LogType.Log:
                            infoCount += item.Value;
                            break;
                        case LogType.Warning:
                            warningCount += item.Value;
                            break;
                        case LogType.Error:
                        case LogType.Assert:
                        case LogType.Exception:
                            errorCount += item.Value;
                            break;
                    }
                }

                bool infoCountIsActive = (filterMode & FilterMode.Info) != 0;
                bool warningCountIsActive = (filterMode & FilterMode.Warning) != 0;
                bool errorCountIsActive = (filterMode & (FilterMode.Error | FilterMode.Assert | FilterMode.Exception)) != 0;

                GUIContent infoCountContent = new GUIContent(infoCount.ToString(), infoIcon);
                GUIContent warningCountContent = new GUIContent(warningCount.ToString(), warningIcon);
                GUIContent errorCountContent = new GUIContent(errorCount.ToString(), errorIcon);

                float infoCountWidth = EditorStyles.toolbarButton.CalcSize(infoCountContent).x;
                float warningCountWidth = EditorStyles.toolbarButton.CalcSize(warningCountContent).x;
                float errorCountWidth = EditorStyles.toolbarButton.CalcSize(errorCountContent).x;

                float maxWidth = Mathf.Max(infoCountWidth, warningCountWidth, errorCountWidth);
                infoCountWidth = maxWidth;
                warningCountWidth = maxWidth;
                errorCountWidth = maxWidth;

                EditorGUI.BeginChangeCheck();
                Rect infoCountPosition = GUILayoutUtility.GetRect(infoCountWidth, 0);
                infoCountPosition.width = infoCountWidth;
                infoCountPosition.height = 20;
                infoCountIsActive = EditorGUI.Toggle(infoCountPosition, infoCountIsActive, EditorStyles.toolbarButton);
                infoCountPosition.x += 2;
                GUI.Label(infoCountPosition, infoCountContent);

                Rect warningCountPosition = GUILayoutUtility.GetRect(warningCountWidth, 0);
                warningCountPosition.width = warningCountWidth;
                warningCountPosition.height = 20;
                warningCountIsActive = EditorGUI.Toggle(warningCountPosition, warningCountIsActive, EditorStyles.toolbarButton);
                warningCountPosition.x += 2;
                GUI.Label(warningCountPosition, warningCountContent);

                Rect errorCountPosition = GUILayoutUtility.GetRect(errorCountWidth, 0);
                errorCountPosition.width = errorCountWidth;
                errorCountPosition.height = 20;
                errorCountIsActive = EditorGUI.Toggle(errorCountPosition, errorCountIsActive, EditorStyles.toolbarButton);
                errorCountPosition.x += 2;
                GUI.Label(errorCountPosition, errorCountContent);

                if (EditorGUI.EndChangeCheck())
                {
                    if (infoCountIsActive)
                    {
                        filterMode |= FilterMode.Info;
                    }
                    else
                    {
                        filterMode &= ~FilterMode.Info;
                    }

                    if (warningCountIsActive)
                    {
                        filterMode |= FilterMode.Warning;
                    }
                    else
                    {
                        filterMode &= ~FilterMode.Warning;
                    }

                    if (errorCountIsActive)
                    {
                        filterMode |= (FilterMode.Error | FilterMode.Assert | FilterMode.Exception);
                    }
                    else
                    {
                        filterMode &= ~(FilterMode.Error | FilterMode.Assert | FilterMode.Exception);
                    }
                }

                filterMode = (FilterMode)EditorGUILayout.EnumFlagsField(filterMode, EditorStyles.toolbarDropDown, GUILayout.Width(17));
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Called every GUI call to draw logs.
        /// </summary>
        private void OnLogsGUI()
        {
            splitView.BeginSplitView();
            {
                if (collapse)
                {
                    DrawLogsCollapsed();
                }
                else
                {
                    DrawAllLogs();
                }
            }
            splitView.Split();
            {
                DrawStackTrace();
            }
            splitView.EndSplitView();
        }

        /// <summary>
        /// Draw all logs.
        /// </summary>
        private void DrawAllLogs()
        {
            int count = 0;
            Event current = Event.current;
            for (int i = 0; i < logs.Count; i++)
            {
                Log log = logs[i];

                if (!FilterLog(log.GetLogType())
                    || (searchText != string.Empty && !log.GetCondition().Contains(searchText)))
                {
                    continue;
                }

                LogStyle style = settings.FindStyle(log.GetLogType());
                Rect position = GUILayoutUtility.GetRect(0, style.GetHeight());
                DrawLog(position, log, style.GetIcon(), current, count++);
            }

            KeyDownReceiver(count, ref current);
        }

        /// <summary>
        /// Draw logs collapsed.
        /// </summary>
        private void DrawLogsCollapsed()
        {
            int count = 0;
            Event current = Event.current;
            foreach (KeyValuePair<Log, int> item in collapseLogs)
            {
                Log log = item.Key;

                if (!FilterLog(log.GetLogType())
                    || (searchText != string.Empty && !log.GetCondition().Contains(searchText)))
                {
                    continue;
                }

                LogStyle style = settings.FindStyle(log.GetLogType());
                Rect position = GUILayoutUtility.GetRect(0, style.GetHeight());
                DrawLog(position, log, style.GetIcon(), current, count++);

                GUIContent content = new GUIContent(item.Value.ToString());
                float height = collapseCountStyle.CalcHeight(content, 20);
                Rect countPosition = new Rect(position.xMax - 25, position.center.y - (height / 2), 20, height);
                GUI.Label(countPosition, content, collapseCountStyle);
            }

            KeyDownReceiver(count, ref current);
        }

        /// <summary>
        /// Draw single log.
        /// </summary>
        /// <param name="position">Rectangle position to drawing log.</param>
        /// <param name="log">Log instance.</param>
        /// <param name="logStyle">Log style.</param>
        /// <param name="current">Reference of current event instance.</param>
        /// <param name="count">Current loop count.</param>
        private void DrawLog(Rect position, Log log, Texture icon, Event current, int count)
        {
            bool isActive = selected == count;

            ClickReceiverHandler(position, log, ref current);

            isActive = EditorGUI.Toggle(position, GUIContent.none, isActive, count % 2 != 0 ? logEvenStyle : logOddStyle);
            if (isActive && selected != count)
            {
                selected = count;
                stackTraceHyperText = log.GetStackTrace();
            }

            string text = log.GetCondition();
            if (settings.ShowLogTime())
            {
                text = $"{log.GetTime()} {text}";
            }

            GUI.Label(position, new GUIContent(text, icon), FindStyle(log.GetLogType()));
        }

        /// <summary>
        /// Draw stack trace selectable text.
        /// </summary>
        private void DrawStackTrace()
        {
            float minHeight = stackTraceStyle.CalcHeight(new GUIContent(stackTraceHyperText), position.width);
            EditorGUILayout.SelectableLabel(stackTraceHyperText, stackTraceStyle, new GUILayoutOption[3]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),
                GUILayout.MinHeight(minHeight)
            });
        }

        /// <summary>
        /// Add log to console.
        /// </summary>
        /// <param name="condition">Log condition text.</param>
        /// <param name="stackTrace">Log stack trace text.</param>
        /// <param name="logType">Log type.</param>
        public void AddLog(string condition, string stackTrace, LogType logType)
        {
            if (string.IsNullOrEmpty(stackTrace))
            {
                stackTrace = condition;
            }

            StringBuilder stringBuilder = new StringBuilder();
            using (StringReader stringReader = new StringReader(stackTrace))
            {
                stackTrace = string.Empty;
                string text = string.Empty;
                while ((text = stringReader.ReadLine()) != null)
                {
                    const string HYPER_LINK_FORMAT = "<a href=\"{0}\" line=\"{1}\" column=\"{2}\">{0}</a>";

                    if (TryParseStackTraceLine(text, out string path, out int line, out int column))
                    {
                        string hyperText = string.Format(HYPER_LINK_FORMAT, path, line, column);
                        text = text.Replace(path, hyperText);
                    }
                    stringBuilder.AppendLine(text);
                }
            }
            stackTrace = stringBuilder.ToString();

            int index = condition.IndexOfAny(new char[] { '\r', '\n' });
            string firstline = index == -1 ? condition : condition.Substring(0, index);
            condition = firstline;

            Log log = new Log(condition, stackTrace, logType);
            logs.Add(log);
            AddCollapseLog(log);
            Repaint();
        }

        /// <summary>
        /// Check log type by filter.
        /// </summary>
        /// <param name="logType">Target log type</param>
        /// <returns>True if is valid for filter. Otherwise false.</returns>
        public bool FilterLog(LogType logType)
        {
            return (logType == LogType.Log && (filterMode & FilterMode.Info) != 0)
                || (logType == LogType.Assert && (filterMode & FilterMode.Assert) != 0)
                || (logType == LogType.Warning && (filterMode & FilterMode.Warning) != 0)
                || (logType == LogType.Error && (filterMode & FilterMode.Error) != 0)
                || (logType == LogType.Exception && (filterMode & FilterMode.Exception) != 0);
        }

        /// <summary>
        /// Open stack trace message in IDE.
        /// </summary>
        /// <param name="stackTraceHyperText">Stack trace text.</param>
        public void OpenStackStrace(string stackTraceHyperText)
        {
            using (StringReader stringReader = new StringReader(stackTraceHyperText))
            {
                stackTraceHyperText = string.Empty;
                string text = string.Empty;
                while ((text = stringReader.ReadLine()) != null)
                {
                    int startIndex = text.IndexOf("href=\"Assets");
                    if(startIndex != -1)
                    {
                        int endIndex = text.IndexOf(".cs\"");
                        if(endIndex != -1)
                        {
                            startIndex += 6;
                            endIndex += 3;
                            string path = text.Substring(startIndex, endIndex - startIndex);

                            startIndex = text.IndexOf("line=\"") + 6;
                            endIndex = startIndex;
                            while (text[endIndex + 1] != '\"')
                            {
                                endIndex++;
                            }

                            string sLine = text.Substring(startIndex, ++endIndex - startIndex);
                            int.TryParse(sLine, out int line);

                            startIndex = text.IndexOf("column=\"") + 8;
                            endIndex = startIndex;
                            while (text[endIndex + 1] != '\"')
                            {
                                endIndex++;
                            }

                            string sColumn = text.Substring(startIndex, ++endIndex - startIndex);
                            int.TryParse(sColumn, out int column);

                            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(path), line, column);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Try to parse source line of string to path to script with line and column.
        /// </summary>
        /// <param name="source">Source string line.</param>
        /// <param name="path">Path to script.</param>
        /// <param name="line">Line of code.</param>
        /// <param name="column">Column of code.</param>
        /// <returns>True if source string parsed. Otherwise false.</returns>
        public bool TryParseStackTraceLine(string source, out string path, out int line, out int column)
        {
            const int INVALID_INDEX = -1;

            path = string.Empty;
            line = INVALID_INDEX;
            column = INVALID_INDEX;

            int startIndex = source.IndexOf("Assets", StringComparison.Ordinal);
            int endIndex = source.IndexOf(".cs", StringComparison.Ordinal);
            if (startIndex != INVALID_INDEX && endIndex != INVALID_INDEX)
            {
                endIndex += 3;
                path = source.Substring(startIndex, endIndex - startIndex);

                if (source.Length > endIndex)
                {
                    startIndex = endIndex;
                    if (source[startIndex] == '(')
                    {
                        endIndex = source.IndexOf(",", StringComparison.Ordinal);
                        int.TryParse(source.Substring(++startIndex, endIndex - startIndex), out line);

                        startIndex = endIndex;
                        endIndex = source.IndexOf(")", StringComparison.Ordinal);
                        int.TryParse(source.Substring(++startIndex, endIndex - startIndex), out column);
                        return true;
                    }
                    else if (source[startIndex] == ':')
                    {
                        int middleIndex = source.IndexOf(":", startIndex, StringComparison.Ordinal);
                        endIndex = source.IndexOf(")", middleIndex, StringComparison.Ordinal);
                        int.TryParse(source.Substring(middleIndex + 1, endIndex - middleIndex - 1), out line);
                        column = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Clear console logs.
        /// </summary>
        public void Clear(bool includingUnityConsole = true)
        {
            logs.Clear();
            collapseLogs.Clear();
            stackTraceHyperText = string.Empty;

            if (includingUnityConsole)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                Type type = assembly.GetType("UnityEditor.LogEntries");
                MethodInfo method = type.GetMethod("Clear");
                method.Invoke(new object(), null);
            }
        }

        /// <summary>
        /// Find log GUI style relative log type.
        /// </summary>
        /// <param name="logType">Target log type.</param>
        public GUIStyle FindStyle(LogType logType)
        {
            switch (logType)
            {
                default:
                case LogType.Log:
                    return infoStyle;
                case LogType.Warning:
                    return warningStyle;
                case LogType.Error:
                    return errorStyle;
                case LogType.Assert:
                    return assertStyle;
                case LogType.Exception:
                    return exceptionStyle;
            }
        }

        /// <summary>
        /// Add log to collapse hash.
        /// </summary>
        /// <param name="log">Log instance.</param>
        private void AddCollapseLog(Log log)
        {
            if (collapseLogs.ContainsKey(log))
            {
                collapseLogs[log]++;
            }
            else
            {
                collapseLogs.Add(log, 1);
            }
        }

        /// <summary>
        /// Called every time to handle clicks on console logs.
        /// </summary>
        /// <param name="position">Rectangle position to handling click.</param>
        /// <param name="current">Current event system instance.</param>
        /// <param name="log">Log message.</param>
        /// <returns>True if log has been clicked. Otherwise false.</returns>
        private void ClickReceiverHandler(Rect position, Log log, ref Event current)
        {
            if (current.type == EventType.MouseDown && position.Contains(current.mousePosition))
            {
                if (current.button == 0 && current.clickCount == 2)
                {
                    OpenStackStrace(log.GetStackTrace());
                }
                else if (current.button == 1)
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("Copy Log"), false, () => EditorGUIUtility.systemCopyBuffer = log.GetCondition());
                    genericMenu.AddItem(new GUIContent("Copy Stack Trace"), false, () => EditorGUIUtility.systemCopyBuffer = log.GetStackTrace());
                    genericMenu.ShowAsContext();
                }
            }
        }

        /// <summary>
        /// Key down receiver handler.
        /// </summary>
        /// <param name="count">Drawed log count.</param>
        /// <param name="current">Current event.</param>
        private void KeyDownReceiver(int count, ref Event current)
        {
            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.UpArrow)
                {
                    selected = Mathf.Max(0, selected - 1);
                }
                else if (current.keyCode == KeyCode.DownArrow)
                {
                    selected = Mathf.Min(count - 1, selected + 1);
                }
                Repaint();
            }
        }

        /// <summary>
        /// Loop through all logs in console.
        /// </summary>
        public IEnumerable<Log> Logs
        {
            get
            {
                for (int i = 0; i < logs.Count; i++)
                {
                    yield return logs[i];
                }
            }
        }

        #region [IHasCustomMenu Implementation]
        /// <summary>
        /// Adds your custom menu items to an Editor Window.
        /// </summary>
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Open Settings"), false, ()=> SettingsService.OpenProjectSettings("Renowned Games/MyConsole"));

            if(logs.Count != 0)
            {
                menu.AddItem(new GUIContent("Export"), false, () =>
                {
                    string path = EditorUtility.SaveFilePanel("Save Console", string.Empty, "ConsoleLogs", "txt");
                    settings.GetConsoleExporter().ExportLogs(path, logs);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Export"));
            }
        }
        #endregion

        #region [Events Callbacks]
        /// <summary>
        /// Event that is fired if a log message is received.
        /// This event will be triggered regardless of whether the message comes in on the main thread or not.
        /// </summary>
        /// <param name="condition">Log condition text.</param>
        /// <param name="stackTrace">Log stack trace text.</param>
        /// <param name="logType">Log type.</param>
        private void ErrorPause(string condition, string stackTrace, LogType logType)
        {
            if (errorPause && EditorApplication.isPlaying && (logType == LogType.Error || logType == LogType.Exception))
            {
                EditorApplication.isPaused = true;
            }
        }

        /// <summary>
        /// Called when changed editor state.
        /// </summary>
        /// <param name="state">Changed play mode state.</param>
        private void ClearOnModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Clear();
            }
        }
        #endregion

        #region [Static Methods]
        /// <summary>
        /// Open MyConsole editor window.
        /// </summary>
        [MenuItem("Renowned Games/MyConsole/MyConsole Window", false, 90)]
        public static void Open()
        {
            if (!HasOpenInstances<MyConsoleWindow>())
            {
                MyConsoleWindow window = GetWindow<MyConsoleWindow>();

                ConsoleSettings settings = ConsoleSettings.GetCurrent();
                Texture icon = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image;
                window.titleContent = new GUIContent(settings.GetTitle(), icon);

                window.MoveToCenter();
                window.Show();
            }
            else
            {
                FocusWindowIfItsOpen<MyConsoleWindow>();
            }

        }

        /// <summary>
        /// Called once after reload scripts.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnRecompile()
        {
            if (HasOpenInstances<MyConsoleWindow>())
            {
                MyConsoleWindow myConsole = GetWindow<MyConsoleWindow>();
                if((myConsole.GetClearMode() & ClearMode.OnRecompile) != 0)
                {
                    myConsole.Clear();
                }
            }
        }

        /// <summary>
        /// Called once after build.
        /// </summary>
        [UnityEditor.Callbacks.PostProcessBuild]
        private static void OnBuild()
        {
            if (HasOpenInstances<MyConsoleWindow>())
            {
                MyConsoleWindow myConsole = GetWindow<MyConsoleWindow>();
                if ((myConsole.GetClearMode() & ClearMode.OnBuild) != 0)
                {
                    myConsole.Clear();
                }
            }
        }
        #endregion

        #region [Getter / Setter]
        public ClearMode GetClearMode()
        {
            return clearMode;
        }

        public void SetClearMode(ClearMode value)
        {
            clearMode = value;
        }

        public FilterMode GetFilterMode()
        {
            return filterMode;
        }

        public void SetFilterMode(FilterMode value)
        {
            filterMode = value;
        }

        public bool Collapse()
        {
            return collapse;
        }

        public void Collapse(bool value)
        {
            collapse = value;
        }

        public bool ErrorPause()
        {
            return errorPause;
        }

        public void ErrorPause(bool value)
        {
            errorPause = value;
        }

        public string GetSearchText()
        {
            return searchText;
        }

        public void SetSearchText(string value)
        {
            searchText = value;
        }

        public int GetSelected()
        {
            return selected;
        }

        public void SetSelected(int value)
        {
            selected = value;
        }

        public string GetStackTraceHyperText()
        {
            return stackTraceHyperText;
        }
        #endregion
    }
}