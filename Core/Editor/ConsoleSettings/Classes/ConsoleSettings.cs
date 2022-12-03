/* ================================================================
   ---------------------------------------------------
   Project   :    MyConsole
   Publisher :    Renowned Games
   Developer :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using RenownedGames.ExLibEditor;
using UnityEngine;

namespace RenownedGames.MyConsoleEditor
{
    public sealed class ConsoleSettings : SettingsSingleton<ConsoleSettings>
    {
        [SerializeField]
        private string title;

        [SerializeField]
        private bool showLogTime;

        [SerializeField]
        private string logTimeFormat;

        [SerializeField]
        private LogStyle infoStyle;

        [SerializeField]
        private LogStyle warningStyle;

        [SerializeField]
        private LogStyle errorStyle;

        [SerializeField]
        private LogStyle assertStyle;

        [SerializeField]
        private LogStyle exceptionStyle;

        [SerializeField]
        private string logEvenStyle;

        [SerializeField]
        private string logOddStyle;

        [SerializeField]
        private string stackTraceStyle;

        [SerializeField]
        private string collapseCountStyle;

        [SerializeField]
        private ConsoleExporter consoleExporter;

        /// <summary>
        /// Find log style relative log type.
        /// </summary>
        /// <param name="logType">Target log type.</param>
        public LogStyle FindStyle(LogType logType)
        {
            switch (logType)
            {
                default:
                case LogType.Log:
                    return GetInfoStyle();
                case LogType.Warning:
                    return GetWarningStyle();
                case LogType.Error:
                    return GetErrorStyle();
                case LogType.Assert:
                    return GetAssertStyle();
                case LogType.Exception:
                    return GetExceptionStyle();
            }
        }

        /// <summary>
        /// Called when the user hits the Reset button in the Inspector's context menu
        /// or when adding the component the first time.
        /// </summary>
        private void Reset()
        {
            title = "MyConsole";
            showLogTime = true;
            logTimeFormat = "[hh:mm:ss]";
            logOddStyle = "CN EntryBackodd";
            logEvenStyle = "CN EntryBackEven";
            stackTraceStyle = "CN Message";
            collapseCountStyle = "CN CountBadge";
            consoleExporter = new ConsoleExporter(ConsoleExporter.InfoFilter.Everything, "----------------------------");
        }

        #region [Getter / Setter]
        public string GetTitle()
        {
            return title;
        }

        public void SetTitle(string value)
        {
            title = value;
        }

        public bool ShowLogTime()
        {
            return showLogTime;
        }

        public void ShowLogTime(bool value)
        {
            showLogTime = value;
        }

        public string GetLogTimeFormat()
        {
            return logTimeFormat;
        }

        public void SetLogTimeFormat(string value)
        {
            logTimeFormat = value;
        }

        public LogStyle GetInfoStyle()
        {
            if(infoStyle == null)
            {
                infoStyle = LogStyle.CreateDefault(LogType.Log);
            }
            return infoStyle;
        }

        public void SetInfoStyle(LogStyle value)
        {
            infoStyle = value;
        }

        public LogStyle GetWarningStyle()
        {
            if (warningStyle == null)
            {
                warningStyle = LogStyle.CreateDefault(LogType.Warning);
            }
            return warningStyle;
        }

        public void SetWarningStyle(LogStyle value)
        {
            warningStyle = value;
        }

        public LogStyle GetErrorStyle()
        {
            if (errorStyle == null)
            {
                errorStyle = LogStyle.CreateDefault(LogType.Error);
            }
            return errorStyle;
        }

        public void SetErrorStyle(LogStyle value)
        {
            errorStyle = value;
        }

        public LogStyle GetAssertStyle()
        {
            if (assertStyle == null)
            {
                assertStyle = LogStyle.CreateDefault(LogType.Error);
            }
            return assertStyle;
        }

        public void SetAssertStyle(LogStyle value)
        {
            assertStyle = value;
        }

        public LogStyle GetExceptionStyle()
        {
            if (exceptionStyle == null)
            {
                exceptionStyle = LogStyle.CreateDefault(LogType.Error);
            }
            return exceptionStyle;
        }

        public void SetExceptionStyle(LogStyle value)
        {
            exceptionStyle = value;
        }

        public string GetLogEvenStyle()
        {
            return logEvenStyle;
        }

        public void SetLogEvenStyle(string value)
        {
            logEvenStyle = value;
        }

        public string GetLogOddStyle()
        {
            return logOddStyle;
        }

        public void SetLogOddStyle(string value)
        {
            logOddStyle = value;
        }

        public string GetStackTraceStyle()
        {
            return stackTraceStyle;
        }

        public void SetStackTraceStyle(string value)
        {
            stackTraceStyle = value;
        }

        public string GetCollapseCountStyle()
        {
            return collapseCountStyle;
        }

        public void SetCollapseCountStyle(string value)
        {
            collapseCountStyle = value;
        }

        public ConsoleExporter GetConsoleExporter()
        {
            return consoleExporter;
        }

        public void SetConsoleExporter(ConsoleExporter value)
        {
            consoleExporter = value;
        }
        #endregion
    }
}